using Microsoft.Azure.Cosmos;

namespace az_rag_search_services.Application.Common.Interfaces;

public interface IAzureCosmosDbService
{
    Task InitializeAsync();
    Container GetContainer();
}