using az_rag_search_services.Application.Abstraction.Messaging;

namespace az_rag_search_services.Application.Features.Note.Query;

public record GetNoteByIdQuery(string Id) : IQuery<GetNoteByIdResult>;                                                       
public record GetNoteByIdResult(Guid Id, string Content);    