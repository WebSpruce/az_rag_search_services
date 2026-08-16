using az_rag_search_services.Application.Abstraction.Messaging;

namespace az_rag_search_services.Application.Features.Note.Query;

public record SearchNotesByVectorQuery(string Content, int Limit) : IQuery<SearchNotesByVectorResult>;                    
public record SearchNotesByVectorResult(IEnumerable<NoteSearchResultDto> Results);                                           
public record NoteSearchResultDto(Guid Id, string Content);         