using MiniDesk.Application.Comments.Dtos;

namespace MiniDesk.Application.Comments;

public interface ICommentService
{
    Task<CommentDto> UpdateAsync(int id, UpdateCommentRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
