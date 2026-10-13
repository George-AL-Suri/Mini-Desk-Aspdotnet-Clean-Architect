using FluentValidation;
using MiniDesk.Application.Comments.Dtos;
using MiniDesk.Domain.Constants;

namespace MiniDesk.Application.Comments.Validators;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator() =>
        RuleFor(x => x.Content).NotEmpty().MaximumLength(CommentConstraints.ContentMaxLength);
}
