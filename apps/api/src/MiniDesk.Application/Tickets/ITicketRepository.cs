using MiniDesk.Domain.Entities;
using MiniDesk.Domain.Enums;

namespace MiniDesk.Application.Tickets;

//write side && tracked entities
public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(int id, CancellationToken ct);
    Task AddAsync(Ticket ticket, CancellationToken ct);
    void Remove(Ticket ticket);
    Task AddCommentAsync(Comment comment, CancellationToken ct);
    Task<bool> CategoryExistsAsync(int categoryId, CancellationToken ct);
    Task<string?> GetCategoryNameAsync(int categoryId, CancellationToken ct);
    void SetRowVersion(Ticket ticket, byte[] rowVersion);
}

