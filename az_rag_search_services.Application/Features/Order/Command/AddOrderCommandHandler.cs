using az_rag_search_services.Application.Abstraction.Messaging;
using az_rag_search_services.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace az_rag_search_services.Application.Features.Order.Command;

public class AddOrderCommandHandler : ICommandHandler<AddOrderCommand, AddOrderResult>
{
    private readonly IOrderMessageSender _orderMessageSender;
    private readonly ILogger<AddOrderCommandHandler> _logger;
    private readonly IValidator<AddOrderCommand> _validator;

    public AddOrderCommandHandler(IOrderMessageSender orderMessageSender, ILogger<AddOrderCommandHandler> logger, IValidator<AddOrderCommand> validator)
    {
        _orderMessageSender = orderMessageSender;
        _logger = logger;
        _validator = validator;
    }

    public async Task<AddOrderResult> Handle(AddOrderCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AddOrderCommandHandler Handle Started");

        cancellationToken.ThrowIfCancellationRequested();

        // disabled -> To test dead-lettering - service bus, enabled -> normal validation
        if (command.ValidationEnabled)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(string.Join("|", validationResult.Errors.Select(e => e.ErrorMessage))); 
        }

        var order = new Domain.Entities.Order(
            Guid.CreateVersion7().ToString(),
            command.CustomerId,
            command.Amount,
            command.Status,
            command.Items);

        await _orderMessageSender.SendMessageAsync(order, cancellationToken);

        _logger.LogInformation("AddOrderCommandHandler Handle Completed");

        return new AddOrderResult(order.OrderId, order.CustomerId, order.Amount, order.Status);
    }
}