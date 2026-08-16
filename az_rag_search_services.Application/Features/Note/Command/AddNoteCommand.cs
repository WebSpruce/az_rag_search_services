using az_rag_search_services.Application.Abstraction.Messaging;

namespace az_rag_search_services.Application.Features.Note.Command;

public record AddNoteCommand(string Content) : ICommand<AddNoteResult>;
public record AddNoteResult(Guid Id, string Content);