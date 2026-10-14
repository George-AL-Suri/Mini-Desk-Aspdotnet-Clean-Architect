using Microsoft.EntityFrameworkCore;
using MiniDesk.Application.Tickets;
using MiniDesk.Domain.Entities;

namespace MiniDesk.Infrastructure.Persistence.Repositories;

internal class TicketRepository(AppDbContext db) : ITicketRepository
{
    // tracked on purpose: the service modifies the entity and calls SaveChanges
    public Task<Ticket?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(Ticket ticket, CancellationToken ct) =>
        await db.Tickets.AddAsync(ticket, ct);

    public void Remove(Ticket ticket) =>
        db.Tickets.Remove(ticket);

    public async Task AddCommentAsync(Comment comment, CancellationToken ct) =>
        await db.Comments.AddAsync(comment, ct);

    public Task<bool> CategoryExistsAsync(int categoryId, CancellationToken ct) =>
        db.Categories.AnyAsync(c => c.Id == categoryId, ct);

    public async Task<string?> GetCategoryNameAsync(int categoryId, CancellationToken ct)
    {
        return await db.Categories
            .Where(c => c.Id == categoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();
    }
    public void SetRowVersion(Ticket ticket, byte[] rowVersion) =>
        db.Entry(ticket).Property(t => t.RowVersion).OriginalValue = rowVersion;
}