using System.Text;
using System.Text.Json;
using az_rag_search_services.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace az_rag_search_services.Infrastructure.Persistence.Services;

public class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly ILogger<OllamaEmbeddingService> _logger;

    public OllamaEmbeddingService(HttpClient httpClient, IConfiguration configuration, ILogger<OllamaEmbeddingService> logger)
    {
        _httpClient = httpClient;
        _model = configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text";
        _logger = logger;

        var baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, EmbeddingTaskType taskType, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("OllamaEmbeddingService GenerateEmbeddingAsync Started");

        // nomic-embed-text expects a task prefix for best retrieval quality
        var prefix = taskType == EmbeddingTaskType.Query ? "search_query: " : "search_document: ";
        var prefixedText = $"{prefix}{text}";

        var payload = new
        {
            model = _model,
            input = prefixedText
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("/api/embed", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseBody);

        // /api/embed returns: { "embeddings": [[...768 floats...]] }
        var embeddingsArray = doc.RootElement.GetProperty("embeddings")[0];
        var vector = new float[embeddingsArray.GetArrayLength()];
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] = embeddingsArray[i].GetSingle();
        }

        _logger.LogInformation("OllamaEmbeddingService GenerateEmbeddingAsync Completed, dimensions: {Dim}", vector.Length);
        return vector;
    }
}