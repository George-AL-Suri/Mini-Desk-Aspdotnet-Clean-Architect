using MiniDesk.Domain.Entities;

namespace MiniDesk.Application.Comments;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(int id, CancellationToken ct);   // tracked, includes Ticket
    void Remove(Comment comment);
}
