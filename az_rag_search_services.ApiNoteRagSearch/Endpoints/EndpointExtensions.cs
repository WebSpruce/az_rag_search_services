using System.Reflection;
using az_rag_search_services.ApiNoteRagSearch.Interfaces;

namespace az_rag_search_services.ApiNoteRagSearch.Endpoints;

public static class EndpointExtensions
{
    public static void RegisterModules(this IServiceCollection services)
    {
        var modules = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && t.IsAssignableTo(typeof(IModule)))
            .Select(Activator.CreateInstance)
            .Cast<IModule>();

        foreach (var module in modules)
        {
            services.AddSingleton(module);
        }
    }

    public static void MapEndpoints(this WebApplication app)
    {
        var modules = app.Services.GetServices<IModule>();
        foreach (var module in modules)
        {
            module.RegisterEndpoints(app);
        }
    }
}