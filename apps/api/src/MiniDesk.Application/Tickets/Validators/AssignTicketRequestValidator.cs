using FluentValidation;
using MiniDesk.Application.Tickets.Dtos;

namespace MiniDesk.Application.Tickets.Validators;

public class AssignTicketRequestValidator : AbstractValidator<AssignTicketRequest>
{
    public AssignTicketRequestValidator()
    {
        RuleFor(x => x.AssignedToId).NotEmpty();
    }
}
