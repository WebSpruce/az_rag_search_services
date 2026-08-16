using FluentValidation;

namespace az_rag_search_services.Application.Features.Note.Command;

public class AddNoteCommandValidator : AbstractValidator<AddNoteCommand>
{
    public AddNoteCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(10_000).WithMessage("Content must be 10,000 characters or fewer.");
    }
}