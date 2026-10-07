using FluentValidation;
using MiniDesk.Application.Tickets.Dtos;

namespace MiniDesk.Application.Tickets.Validators;

public class ChangeTicketStatusRequestValidator : AbstractValidator<ChangeTicketStatusRequest>
{
    public ChangeTicketStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
