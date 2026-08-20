using az_rag_search_services.Domain.Entities;
using az_rag_search_services.Domain.Worker.Services;

namespace az_rag_search_services.Worker.Services;

public class OrderProcessor : IOrderProcessor
{
    private readonly ILogger<OrderProcessor> _logger;
    private static readonly string[] ValidStatuses = { "Pending", "Processing", "Completed", "Cancelled" };

    public OrderProcessor(ILogger<OrderProcessor> logger)
    {
        _logger = logger;
    }

    public Task ProcessAsync(Order order, CancellationToken cancellationToken)
    {
        _logger.LogInformation("OrderProcessor ProcessAsync Started for OrderId: {OrderId}", order.OrderId);

        // Immediate, explicit dead-letter conditions
        // These are permanently invalid; retrying will never help,
        // so fail fast instead of burning delivery attempts.
        if (string.IsNullOrWhiteSpace(order.CustomerId))
        {
            throw new InvalidDataException($"MissingCustomerId: Order {order.OrderId} has no CustomerId.");
        }

        if (!ValidStatuses.Contains(order.Status))
        {
            throw new InvalidDataException($"InvalidOrderStatus: Order {order.OrderId} has unrecognized status '{order.Status}'.");
        }

        if (order.Items is null || order.Items.Count == 0)
        {
            throw new InvalidDataException($"EmptyOrderItems: Order {order.OrderId} has no line items.");
        }

        foreach (var item in order.Items)
        {
            if (item.Quantity <= 0 || item.Price < 0)
            {
                throw new InvalidDataException(
                    $"InvalidOrderItem: Order {order.OrderId} has invalid item {item.ProductId} (Quantity={item.Quantity}, Price={item.Price}).");
            }
        }

        // Simulated transient/business failure
        // Negative Amount is treated as a processing failure that
        // *should* retry, this drives MaxDeliveryCountExceeded
        // instead of an immediate dead-letter.
        if (order.Amount <= 0)
        {
            throw new InvalidOperationException(
                $"Order {order.OrderId} has non-positive amount {order.Amount}; simulated processing failure.");
        }

        _logger.LogInformation("OrderProcessor ProcessAsync Completed for OrderId: {OrderId}, Amount: {Amount}",
            order.OrderId, order.Amount);

        return Task.CompletedTask;
    }
}