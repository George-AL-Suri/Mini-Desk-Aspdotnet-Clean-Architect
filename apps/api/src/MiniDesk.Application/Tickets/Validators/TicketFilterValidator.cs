using FluentValidation;
using MiniDesk.Application.Tickets.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniDesk.Application.Tickets.Validators;

public class TicketFilterValidator : AbstractValidator<TicketFilter>
{
    public TicketFilterValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortBy).IsInEnum();
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.Priority).IsInEnum().When(x => x.Priority.HasValue);
        RuleFor(x => x.Search).MaximumLength(100);
    }
}
