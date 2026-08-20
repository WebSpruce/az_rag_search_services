using System.ComponentModel.DataAnnotations;
using az_rag_search_services.ApiNoteRagSearch.Interfaces;
using az_rag_search_services.Application.Abstraction.Messaging;
using az_rag_search_services.Application.Features.Order.Command;

namespace az_rag_search_services.ApiNoteRagSearch.Endpoints;

public class OrderEndpoints : IModule
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var notes = app
            .MapGroup(ApiRoutes.Orders.GroupName)
            .WithTags("Orders")
            .WithApiVersionSet(ApiRoutes.ApiVersion(app));
        
         notes.MapPost("/add", async (
            AddOrderCommand command, 
            ICommandHandler<AddOrderCommand, AddOrderResult> handler,
            CancellationToken token
            ) =>
            {
                try
                {
                    var result = await handler.Handle(command, token);
                    return Results.Created($"/api/order/{result.Id}", result);
                }
                catch (ValidationException ex)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["ValidationErrors"] = ex.Message.Split('|')
                    });
                }
            })
            .Produces<AddOrderResult>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }
}