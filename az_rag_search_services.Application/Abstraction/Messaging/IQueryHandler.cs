namespace az_rag_search_services.Application.Abstraction.Messaging;

public interface IQueryHandler<in TQuery, TResponse>
where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken token);
}