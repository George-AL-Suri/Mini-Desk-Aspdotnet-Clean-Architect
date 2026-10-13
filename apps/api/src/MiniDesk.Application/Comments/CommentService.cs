using FluentValidation;
using MiniDesk.Application.Comments.Dtos;
using MiniDesk.Application.Common.Constants;
using MiniDesk.Application.Common.Exceptions;
using MiniDesk.Application.Common.Interfaces;
using MiniDesk.Domain.Entities;
using MiniDesk.Domain.Enums;

namespace MiniDesk.Application.Comments;

public class CommentService(
    ICommentRepository comments,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IValidator<UpdateCommentRequest> updateValidator) : ICommentService
{
    public async Task<CommentDto> UpdateAsync(int id, UpdateCommentRequest request, CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);

        var comment = await LoadAsync(id, ct);

        if (comment.AuthorId != CurrentUserId)
            throw new ForbiddenException("Only the author can edit a comment.");

        if (comment.Ticket.Status == TicketStatus.Closed)
            throw new ConflictException("Comments on a closed ticket cannot be edited.");

        comment.Content = request.Content;
        await unitOfWork.SaveChangesAsync(ct);   // UpdatedAt set by the SaveChanges override

        // the editor is the author, so the name comes from the token (no extra query)
        return new CommentDto(
            comment.Id,
            comment.TicketId,
            comment.Content,
            comment.AuthorId,
            currentUser.UserName ?? "",
            comment.CreatedAt);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var comment = await LoadAsync(id, ct);

        var isAuthor = comment.AuthorId == CurrentUserId;
        if (!isAuthor && !currentUser.IsInRole(Roles.Admin))
            throw new ForbiddenException();

        comments.Remove(comment);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private Guid CurrentUserId => currentUser.UserId ?? throw new ForbiddenException();

    private async Task<Comment> LoadAsync(int id, CancellationToken ct) =>
        await comments.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Comment), id);
}