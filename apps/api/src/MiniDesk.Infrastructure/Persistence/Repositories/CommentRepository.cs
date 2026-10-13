using Microsoft.EntityFrameworkCore;
using MiniDesk.Application.Comments;
using MiniDesk.Domain.Entities;

namespace MiniDesk.Infrastructure.Persistence.Repositories;

internal class CommentRepository(AppDbContext db) : ICommentRepository
{
    public Task<Comment?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Comments
            .Include(c => c.Ticket)               // needed for the closed-ticket check
            .FirstOrDefaultAsync(c => c.Id == id, ct);   // tracked on purpose

    public void Remove(Comment comment) => db.Comments.Remove(comment);
}