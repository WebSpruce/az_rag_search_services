using az_rag_search_services.Domain.Worker.Services;
using az_rag_search_services.Worker.Jobs;
using az_rag_search_services.Worker.Services;
using Azure.Messaging.ServiceBus;

namespace az_rag_search_services.Worker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddDebug();
        });
        
        builder.Services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("ServiceBusConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'ServiceBusConnectionString' is not configured. " +
                    "Set CONNECTIONSTRINGS__ServiceBusConnectionString environment variable.");
            }
            return new ServiceBusClient(connectionString);
        });

        builder.Services.AddSingleton<IOrderProcessor, OrderProcessor>();
        builder.Services.AddHostedService<OrderProcessingWorker>();

        var host = builder.Build();
        host.Run();
    }
}