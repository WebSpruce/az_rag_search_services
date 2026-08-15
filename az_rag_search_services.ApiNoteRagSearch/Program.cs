using Asp.Versioning;
using az_rag_search_services.ApiNoteRagSearch.Endpoints;
using az_rag_search_services.Application.Common.Interfaces;
using az_rag_search_services.Infrastructure;

namespace az_rag_search_services.ApiNoteRagSearch;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.Services.AddInfrastructure(builder.Configuration);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });
        builder.Services.RegisterModules();
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        
        using (var scope = app.Services.CreateScope())
        {
            var cosmosService = scope.ServiceProvider.GetRequiredService<IAzureCosmosDbService>();                                   
            await cosmosService.InitializeAsync(); 
        }
        
        app.MapEndpoints();

        app.Run();
    }
}