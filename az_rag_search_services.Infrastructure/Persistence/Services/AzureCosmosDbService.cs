using System.Collections.ObjectModel;
using az_rag_search_services.Application.Common.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace az_rag_search_services.Infrastructure.Persistence.Services;

public class AzureCosmosDbService : IAzureCosmosDbService
{
    private readonly string _databaseName;                                                                                   
    private readonly string _containerName;                                                                                  
    private readonly int _dimensions;                                                                                        
    private readonly CosmosClient _cosmosClient;                                                                             
    private Container? _container;
    
    private readonly ILogger<AzureCosmosDbService> _logger;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
                                                                                                                             
    public AzureCosmosDbService(IConfiguration configuration, CosmosClient cosmosClient, ILogger<AzureCosmosDbService> logger)                                     
    {                                                                                                                        
        _databaseName = configuration["CosmosDb:DatabaseName"] ?? throw new ArgumentNullException("CosmosDb:DatabaseName is issing");                                                                                                                   
        _containerName = configuration["CosmosDb:ContainerName"] ?? throw new ArgumentNullException("CosmosDb:ContainerName is missing");                                                                                                                
        _dimensions = int.Parse(configuration["CosmosDb:Dimensions"] ?? "1536");                                             
                                                                                                                             
        _cosmosClient = cosmosClient;
        _logger = logger;
    }
    
    public async Task InitializeAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _logger.LogInformation($"AzureCosmosDbService InitializeAsync Started");
            var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseName);

            ContainerProperties containerProperties =
                new ContainerProperties(id: _containerName, partitionKeyPath: "/id")
                {
                    VectorEmbeddingPolicy = new VectorEmbeddingPolicy(
                        new Collection<Embedding>
                        {
                            new Embedding()
                            {
                                Path = "/vectorEmbedding",
                                DataType = VectorDataType.Float32,
                                Dimensions = _dimensions,
                                DistanceFunction = DistanceFunction.Cosine
                            }
                        })
                };
            
            containerProperties.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/*" });
            containerProperties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = "/vectorEmbedding/*" });

            containerProperties.IndexingPolicy.VectorIndexes.Add(
                new VectorIndexPath()
                {
                    Path = "/vectorEmbedding",
                    Type = VectorIndexType.DiskANN
                });

            _container = await database.Database.CreateContainerIfNotExistsAsync(containerProperties);
            _logger.LogInformation($"AzureCosmosDbService InitializeAsync Completed");
        }
        catch (CosmosException ex)                                                                                                   
        {                                                                                                                            
            _logger.LogError($"AzureCosmosDbService InitializeAsync Cosmos DB Error: Status: {ex.StatusCode}, Substatus: {ex.SubStatusCode}, ResponseBody: {ex.ResponseBody}, Message: {ex.Message}");     
        } 
        catch (Exception ex)
        {
            _logger.LogError($"AzureCosmosDbService InitializeAsync method: {ex.Message} - {ex.InnerException}");
        }
        finally
        {
            _semaphore.Release(); 
        }
                                      
    }                                                                                                                        
    
    public Container GetContainer()                                                                                          
    {                                                                                                                        
        return _container ?? throw new InvalidOperationException("Service not initialized. Call InitializeAsync first.");    
    }                            
    
}