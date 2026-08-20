using az_rag_search_services.Application.Common.Interfaces;
using az_rag_search_services.Domain.Entities;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace az_rag_search_services.Infrastructure.Persistence.Services;

public class ServiceBusOrderSender : IOrderMessageSender
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusOrderSender> _logger;

    public ServiceBusOrderSender(ServiceBusClient client, IConfiguration configuration, ILogger<ServiceBusOrderSender> logger)
    {
        var queueName = configuration["ServiceBus:OrderQueueName"] ?? throw new ArgumentNullException("ServiceBus:OrderQueueName is missing");
        _sender = client.CreateSender(queueName);
        _logger = logger;
    }

    public async Task SendMessageAsync(Order order, CancellationToken token = default)
    {
        try
        {
            _logger.LogInformation($"ServiceBusOrderSender SendMessageAsync Started");

            var message = new ServiceBusMessage(BinaryData.FromObjectAsJson(order));
            await _sender.SendMessageAsync(message, token);
            
            _logger.LogInformation($"ServiceBusOrderSender SendMessageAsync Completed");
        }
        catch (Exception ex)
        {
            _logger.LogError($"ServiceBusOrderSender SendMessageAsync method: {ex.Message} - {ex.InnerException}");
            throw;
        }
    }
}