using FluentValidation;
using MiniDesk.Application.Tickets.Dtos;
using MiniDesk.Domain.Constants;
using System.Buffers.Text;

namespace MiniDesk.Application.Tickets.Validators;

public class UpdateTicketRequestValidator : AbstractValidator<UpdateTicketRequest>
{
    public UpdateTicketRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(TicketConstraints.TitleMaxLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(TicketConstraints.DescriptionMaxLength);
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.RowVersion).NotEmpty().Must(IsBase64).WithMessage("Invalid row version.");
    }
    private static bool IsBase64(string v) => Convert.TryFromBase64String(v, new byte[v.Length], out _);
}
