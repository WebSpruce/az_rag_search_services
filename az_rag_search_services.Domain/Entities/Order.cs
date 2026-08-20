namespace az_rag_search_services.Domain.Entities;

public record Order(
    string OrderId,
    string CustomerId,
    decimal Amount,
    string Status,
    List<OrderItem> Items
);

public record OrderItem(string ProductId, int Quantity, decimal Price);