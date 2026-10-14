using FluentValidation;
using MiniDesk.Application.Auth.Dtos;

namespace MiniDesk.Application.Auth.Validators;

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
