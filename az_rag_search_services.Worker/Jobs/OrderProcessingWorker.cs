using System.Text.Json;
using az_rag_search_services.Domain.Entities;
using az_rag_search_services.Domain.Worker.Services;
using Azure.Messaging.ServiceBus;

namespace az_rag_search_services.Worker.Jobs;

public class OrderProcessingWorker : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IOrderProcessor _orderProcessor;
    private readonly ILogger<OrderProcessingWorker> _logger;
    private readonly string _queueName;
    private readonly int _maxConcurrentCalls;
    private ServiceBusProcessor? _processor;

    public OrderProcessingWorker(
        ServiceBusClient client,
        IOrderProcessor orderProcessor,
        IConfiguration configuration,
        ILogger<OrderProcessingWorker> logger)
    {
        _client = client;
        _orderProcessor = orderProcessor;
        _logger = logger;

        _queueName = configuration["ServiceBus:OrderQueueName"]
            ?? throw new ArgumentNullException("ServiceBus:OrderQueueName is missing");
        _maxConcurrentCalls = int.TryParse(configuration["ServiceBus:MaxConcurrentCalls"], out var n) ? n : 1;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"OrderProcessingBackgroundService starting on queue: {_queueName}");

        var options = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = _maxConcurrentCalls,
            AutoCompleteMessages = false
        };

        _processor = _client.CreateProcessor(_queueName, options);

        _processor.ProcessMessageAsync += HandleMessageAsync;
        _processor.ProcessErrorAsync += HandleErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);

        // Keep running until cancellation is requested
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            
        }

        await _processor.StopProcessingAsync(stoppingToken);
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        _logger.LogInformation($"Received message {message.MessageId}, DeliveryCount: {message.DeliveryCount}");

        Order? order;
        try
        {
            order = JsonSerializer.Deserialize<Order>(message.Body.ToString());
        }
        catch (JsonException ex)
        {
            // poison JSON — never retry-able, dead-letter immediately.
            _logger.LogError(ex, $"Failed to deserialize message {message.MessageId}");
            await args.DeadLetterMessageAsync(
                message,
                deadLetterReason: "PoisonJson",
                deadLetterErrorDescription: ex.Message,
                cancellationToken: args.CancellationToken);
            return;
        }

        if (order is null)
        {
            await args.DeadLetterMessageAsync(
                message,
                deadLetterReason: "NullOrderPayload",
                deadLetterErrorDescription: "Deserialized order was null.",
                cancellationToken: args.CancellationToken);
            return;
        }

        try
        {
            await _orderProcessor.ProcessAsync(order, args.CancellationToken);

            await args.CompleteMessageAsync(message, args.CancellationToken);
            _logger.LogInformation("Order {OrderId} processed and completed.", order.OrderId);
        }
        catch (InvalidDataException ex)
        {
            // dead-letter now
            var reason = ex.Message.Split(':')[0];
            _logger.LogWarning($"Order {order.OrderId} explicitly dead-lettered. Reason: {reason}");

            await args.DeadLetterMessageAsync(
                message,
                deadLetterReason: reason,
                deadLetterErrorDescription: ex.Message,
                cancellationToken: args.CancellationToken);
        }
        catch (Exception ex)
        {
            // abandon so it becomes available for redelivery.
            // Once DeliveryCount exceeds the queue's MaxDeliveryCount, Service Bus
            // will move it to the DLQ automatically with reason MaxDeliveryCountExceeded.
            _logger.LogError(ex, $"Transient failure processing order {order.OrderId}. Abandoning for retry.");
            await args.AbandonMessageAsync(message, cancellationToken: args.CancellationToken);
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, $"ServiceBusProcessor error in source {args.ErrorSource}");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }
        await base.StopAsync(cancellationToken);
    }
}