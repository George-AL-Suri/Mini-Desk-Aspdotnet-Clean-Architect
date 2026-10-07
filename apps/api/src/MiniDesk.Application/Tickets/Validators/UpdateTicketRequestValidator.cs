using FluentValidation;
using MiniDesk.Application.Tickets.Dtos;
using MiniDesk.Domain.Constants;

namespace MiniDesk.Application.Tickets.Validators;

public class UpdateTicketRequestValidator : AbstractValidator<UpdateTicketRequest>
{
    public UpdateTicketRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(TicketConstraints.TitleMaxLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(TicketConstraints.DescriptionMaxLength);
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.CategoryId).GreaterThan(0);
    }
}
