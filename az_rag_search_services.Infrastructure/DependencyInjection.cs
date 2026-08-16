using az_rag_search_services.Application.Common.Interfaces;
using az_rag_search_services.Infrastructure.Persistence.Repositories;
using az_rag_search_services.Infrastructure.Persistence.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace az_rag_search_services.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PGConnection");
        // if (string.IsNullOrEmpty(connectionString))
        // {
        //     throw new InvalidOperationException(
        //         "Connection string 'PGConnection' is not configured. " +
        //         "Set CONNECTIONSTRINGS__PGCONNECTION environment variable.");
        // }

        //postgresql
        // var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        // dataSourceBuilder.UseVector();
        // var dataSource = dataSourceBuilder.Build();
        // services.AddDbContext<AppDbContext>(options =>
        // {
        //     options.UseNpgsql(dataSource, o => o.UseVector());
        // });
        
        //az cosmosdb
        services.AddSingleton(sp =>
        {
            connectionString = configuration.GetConnectionString("CosmosDbConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'CosmosDbConnectionString' is not configured. " +
                    "Set CONNECTIONSTRINGS__CosmosDbConnectionString environment variable.");
            }
            return new CosmosClient(connectionString, new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                },
                HttpClientFactory = () =>
                {
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                    return new HttpClient(handler);
                },
                ConnectionMode = ConnectionMode.Gateway
            });
        });

        services.AddSingleton<IAzureCosmosDbService, AzureCosmosDbService>();
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>();
        
        return services;
    }
}