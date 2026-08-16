using az_rag_search_services.ApiNoteRagSearch.Interfaces;
using az_rag_search_services.Application.Abstraction.Messaging;
using az_rag_search_services.Application.Features.Note.Command;
using az_rag_search_services.Application.Features.Note.Query;

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
            AddNoteCommand command, 
            ICommandHandler<AddNoteCommand, AddNoteResult> handler, 
            CancellationToken token
            ) =>
        {
            var result = await handler.Handle(command, token);
            return Results.Created($"/api/notes/{result.Id}", result);
        })
            .Produces<AddNoteResult>(StatusCodes.Status201Created)
            .ProducesValidationProblem();;
        
        notes.MapGet("/{id}", async (                                                                                                
            string id,                                                                                                               
            IQueryHandler<GetNoteByIdQuery, GetNoteByIdResult> handler,                                                              
            CancellationToken token                                                                                                  
            ) =>                                                                                                                     
        {                                                                                                                            
            try
            {                                                                                                                        
                var result = await handler.Handle(new GetNoteByIdQuery(id), token);                                                  
                return Results.Ok(result);                                                                                           
            }                                                                                                                        
            catch (KeyNotFoundException)                                                                                             
            {                                                                                                                        
                return Results.NotFound();                                                                                           
            }                                                                                                                        
        })                                                                                                                           
        .Produces<GetNoteByIdResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);      
        
        notes.MapPost("/search", async (                                                                                             
                SearchNotesByVectorQuery query,                                                                                          
                IQueryHandler<SearchNotesByVectorQuery, SearchNotesByVectorResult> handler,                                              
                CancellationToken token                                                                                                  
            ) =>                                                                                                                     
            {                                                                                                                            
                var result = await handler.Handle(query, token);                                                                         
                return Results.Ok(result);                                                                                               
            })                                                                                                                           
            .Produces<SearchNotesByVectorResult>(StatusCodes.Status200OK)                                                                
            .WithName("SearchNotesByVector");    
        
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