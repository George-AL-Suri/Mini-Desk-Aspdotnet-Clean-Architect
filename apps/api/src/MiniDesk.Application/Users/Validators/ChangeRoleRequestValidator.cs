using FluentValidation;
using MiniDesk.Application.Common.Constants;
using MiniDesk.Application.Users.Dtos;

namespace MiniDesk.Application.Users.Validators;

public class ChangeRoleRequestValidator : AbstractValidator<ChangeRoleRequest>
{
    public ChangeRoleRequestValidator() =>
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => Roles.All.Contains(r))
            .WithMessage($"Role must be one of: {string.Join(", ", Roles.All)}.");
}
