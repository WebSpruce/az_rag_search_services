using az_rag_search_services.Application.Abstraction.Messaging;
using az_rag_search_services.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace az_rag_search_services.Application.Features.Note.Query;

public class GetNoteByIdQueryHandler : IQueryHandler<GetNoteByIdQuery, GetNoteByIdResult>                                    
{                                                                                                                            
    private readonly INoteRepository _noteRepository;                                                                        
    private readonly ILogger<GetNoteByIdQueryHandler> _logger;                                                               
                                                                                                                             
    public GetNoteByIdQueryHandler(INoteRepository noteRepository, ILogger<GetNoteByIdQueryHandler> logger)                  
    {                                                                                                                        
        _noteRepository = noteRepository;                                                                                    
        _logger = logger;                                                                                                    
    }                                                                                                                        
                                                                                                                             
    public async Task<GetNoteByIdResult> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)               
    {                                                                                                                        
        _logger.LogInformation("GetNoteByIdQueryHandler Handle Started for Id: {Id}", request.Id);                           
                                                                                                                             
        var note = await _noteRepository.GetByIdAsync(request.Id);                                                          
                                                                                                                             
        if (note == null)                                                                                                    
        {                                                                                                                    
            _logger.LogWarning("Note with Id: {Id} not found.", request.Id);                                                 
            throw new KeyNotFoundException($"Note with ID {request.Id} was not found.");                                     
        }                                                                                                                    
                                                                                                                             
        _logger.LogInformation("GetNoteByIdQueryHandler Handle Completed");                                                  
                                                                                                                             
        return new GetNoteByIdResult(note.Id, note.Content);                                                                 
    }                                                                                                                        
}                        