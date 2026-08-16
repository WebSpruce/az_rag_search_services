using az_rag_search_services.Application.Common.Interfaces;
using az_rag_search_services.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace az_rag_search_services.Infrastructure.Persistence.Repositories;

public class NoteRepository : INoteRepository
{
    private readonly Container _container;
    private readonly ILogger<NoteRepository> _logger;
    public NoteRepository(IAzureCosmosDbService azureCosmosDbService, ILogger<NoteRepository> logger)
    {
        _container = azureCosmosDbService.GetContainer();
        _logger = logger;
    }                                                                                                                        
                                                                                                                             
    public async Task<Note?> GetByIdAsync(string id)                                                                         
    {
        try
        {
            _logger.LogInformation($"NoteRepository GetByIdAsync Started");
            ItemResponse<Note> response = await _container.ReadItemAsync<Note>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogError($"NoteRepository GetByIdAsync method: {ex.Message} - {ex.InnerException}");
            return null;
        }
        finally
        {
            _logger.LogInformation($"NoteRepository GetByIdAsync Completed");
        }
    }                                                                                                                        
                                                                                                                             
    public async Task AddAsync(Note note)                                                                                    
    {
        try
        {
            _logger.LogInformation($"NoteRepository AddAsync Started");
            await _container.CreateItemAsync(note, new PartitionKey(note.Id.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError($"NoteRepository AddAsync method: {ex.Message} - {ex.InnerException}");
            throw;
        }
        finally
        {
            _logger.LogInformation($"NoteRepository AddAsync Completed");
        }
    }                                                                                                                        
                                                                                                                             
    public async Task<IEnumerable<Note>> SearchByVectorAsync(float[] embedding, int limit)
    {
        try
        {
            _logger.LogInformation($"NoteRepository SearchByVectorAsync Started");
            
            // ReSharper disable LanguageInjection
            var query = $@"
                SELECT TOP {limit} c.id, c.content, VectorDistance(c.vectorEmbedding, @vectorLiteral) AS SimilarityScore
                FROM c
                ORDER BY VectorDistance(c.vectorEmbedding, @vectorLiteral)";

            // ReSharper disable LanguageInjection
            var queryDefinition = new QueryDefinition(query)
                .WithParameter("@vectorLiteral", embedding);

            var results = new List<Note>();
            using var iterator = _container.GetItemQueryIterator<Note>(queryDefinition);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError($"NoteRepository SearchByVectorAsync method: {ex.Message} - {ex.InnerException}");
            return Enumerable.Empty<Note>();
        }
        finally
        {
            _logger.LogInformation($"NoteRepository SearchByVectorAsync Completed");
        }
    }                    
}