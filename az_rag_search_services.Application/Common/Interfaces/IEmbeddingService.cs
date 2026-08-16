namespace az_rag_search_services.Application.Common.Interfaces;

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text, EmbeddingTaskType taskType, CancellationToken cancellationToken = default);
}

public enum EmbeddingTaskType
{
    Document,
    Query
}