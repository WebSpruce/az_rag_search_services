using az_rag_search_services.Application.Abstraction.Messaging;
using az_rag_search_services.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace az_rag_search_services.Application.Features.Note.Command;

public class AddNoteCommandHandler : ICommandHandler<AddNoteCommand, AddNoteResult>
{
    private readonly INoteRepository _noteRepository;
    private readonly ILogger<AddNoteCommandHandler> _logger;
    private readonly IValidator<AddNoteCommand> _validator;

    public AddNoteCommandHandler(INoteRepository noteRepository, ILogger<AddNoteCommandHandler> logger, IValidator<AddNoteCommand> validator)
    {
        _noteRepository = noteRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<AddNoteResult> Handle(AddNoteCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AddNoteCommandHandler Handle Started");

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Note content cannot be empty.", nameof(request.Content));
        }
        
        cancellationToken.ThrowIfCancellationRequested();
        
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ArgumentException(validationResult.ToDictionary().ToString()); 

        var note = new Domain.Entities.Note(request.Content);

        await _noteRepository.AddAsync(note);

        _logger.LogInformation("AddNoteCommandHandler Handle Completed");

        return new AddNoteResult(note.Id, note.Content);
    }
}