using FluentValidation;

namespace az_rag_search_services.Application.Features.Order.Command;

public class AddOrderCommandValidator : AbstractValidator<AddOrderCommand>
{
    public AddOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Status).NotEmpty().Must(BeAValidStatus);
        RuleFor(x => x.Items).NotNull();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new OrderItemValidator());
    }

    private bool BeAValidStatus(string status) =>
        new[] { "Pending", "Processing", "Completed", "Cancelled" }.Contains(status);
}