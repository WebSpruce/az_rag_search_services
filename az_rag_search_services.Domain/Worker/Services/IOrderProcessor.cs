using az_rag_search_services.Domain.Entities;

namespace az_rag_search_services.Domain.Worker.Services;

public interface IOrderProcessor
{
    Task ProcessAsync(Order order, CancellationToken token);
}