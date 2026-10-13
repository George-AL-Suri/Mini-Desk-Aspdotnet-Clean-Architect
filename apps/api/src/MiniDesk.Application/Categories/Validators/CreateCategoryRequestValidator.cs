using FluentValidation;
using MiniDesk.Application.Categories.Dtos;
using MiniDesk.Domain.Constants;

namespace MiniDesk.Application.Categories.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator() =>
        RuleFor(x => x.Name).NotEmpty().MaximumLength(CategoryConstraints.NameMaxLength);
}
