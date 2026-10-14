using FluentValidation;
using MiniDesk.Application.Auth.Dtos;

namespace MiniDesk.Application.Auth.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        // Identity enforces upper/lower/digit/symbol and returns its own errors
    }
}
