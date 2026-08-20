using az_rag_search_services.Domain.Entities;

namespace az_rag_search_services.Application.Common.Interfaces;

public interface IOrderMessageSender
{
    Task SendMessageAsync(Order order, CancellationToken token = default);
}