using FluentValidation;
using MiniDesk.Application.Categories.Dtos;
using MiniDesk.Domain.Constants;

namespace MiniDesk.Application.Categories.Validators;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator() =>
        RuleFor(x => x.Name).NotEmpty().MaximumLength(CategoryConstraints.NameMaxLength);
}
