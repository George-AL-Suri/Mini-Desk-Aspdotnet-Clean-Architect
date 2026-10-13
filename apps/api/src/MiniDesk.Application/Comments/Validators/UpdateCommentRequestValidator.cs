using FluentValidation;
using MiniDesk.Application.Comments.Dtos;
using MiniDesk.Domain.Constants;

namespace MiniDesk.Application.Comments.Validators;

public class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentRequestValidator() =>
        RuleFor(x => x.Content).NotEmpty().MaximumLength(CommentConstraints.ContentMaxLength);
}
