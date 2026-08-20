using az_rag_search_services.Application.Abstraction.Messaging;
using az_rag_search_services.Domain.Entities;

namespace az_rag_search_services.Application.Features.Order.Command;

public record AddOrderCommand(
    string CustomerId,
    decimal Amount,
    string Status,
    List<OrderItem> Items,
    bool ValidationEnabled = true) : ICommand<AddOrderResult>;
public record AddOrderResult(string Id, string CustomerId, decimal Amount, string Status);