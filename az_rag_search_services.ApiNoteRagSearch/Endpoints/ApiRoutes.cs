using Asp.Versioning;
using Asp.Versioning.Builder;

namespace az_rag_search_services.ApiNoteRagSearch.Endpoints;

public class ApiRoutes
{
    private const string Base = "/api";
    private const string Version = "v{version:apiVersion}";
    private const string ApiBase = $"{Base}/{Version}";

    public static class Notes
    {
        public const string GroupName = $"{ApiBase}/notes";
    }
    public static class Orders
    {
        public const string GroupName = $"{ApiBase}/orders";
    }
    public static ApiVersionSet ApiVersion(IEndpointRouteBuilder app)
    {
        return app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .Build();
    } 
}