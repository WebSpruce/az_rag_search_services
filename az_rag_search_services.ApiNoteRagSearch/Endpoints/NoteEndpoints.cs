using az_rag_search_services.ApiNoteRagSearch.Interfaces;

namespace az_rag_search_services.ApiNoteRagSearch.Endpoints;

public class NoteEndpoints : IModule
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var notes = app
            .MapGroup(ApiRoutes.Notes.GroupName)
            .WithTags("Notes")
            .WithApiVersionSet(ApiRoutes.ApiVersion(app));

        notes.MapPost("", async (
            
            ) =>
        {

        });
        
        notes.MapGet("", async (
            
        ) =>
        {

        });
        
        notes.MapPatch("", async (
            
        ) =>
        {

        });
        
        notes.MapDelete("", async (
            
        ) =>
        {

        });
    }
}