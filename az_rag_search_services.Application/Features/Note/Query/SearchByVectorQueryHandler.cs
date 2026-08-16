using az_rag_search_services.Application.Abstraction.Messaging;
using az_rag_search_services.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace az_rag_search_services.Application.Features.Note.Query;

public class SearchNotesByVectorQueryHandler : IQueryHandler<SearchNotesByVectorQuery, SearchNotesByVectorResult>            
{                                                                                                                            
    private readonly INoteRepository _noteRepository;                                                                        
    private readonly ILogger<SearchNotesByVectorQueryHandler> _logger;      
    private readonly IEmbeddingService _embeddingService;
                                                                                                                             
    public SearchNotesByVectorQueryHandler(INoteRepository noteRepository, ILogger<SearchNotesByVectorQueryHandler> logger, IEmbeddingService embeddingService)  
    {                                                                                                                        
        _noteRepository = noteRepository;                                                                                    
        _logger = logger;
        _embeddingService = embeddingService;
    }                                                                                                                        
                                                                                                                             
    public async Task<SearchNotesByVectorResult> Handle(SearchNotesByVectorQuery request, CancellationToken cancellationToken)                                                                                                           
    {                                                                                                                        
        _logger.LogInformation("SearchNotesByVectorQueryHandler Handle Started");     
        
        var embedding = await _embeddingService.GenerateEmbeddingAsync(request.Content, EmbeddingTaskType.Query, cancellationToken);
                                                                                                                             
        var notes = await _noteRepository.SearchByVectorAsync(embedding, request.Limit);                             
                                                                                                                             
        var result = new SearchNotesByVectorResult(                                                                          
            notes.Select(n => new NoteSearchResultDto(n.Id, n.Content))                                                      
        );                                                                                                                   
                                                                                                                             
        _logger.LogInformation("SearchNotesByVectorQueryHandler Handle Completed");                                          
                                                                                                                             
        return result;                                                                                                       
    }                                                                                                                        
}   