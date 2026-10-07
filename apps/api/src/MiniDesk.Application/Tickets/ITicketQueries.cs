using MiniDesk.Application.Comments.Dtos;
using MiniDesk.Application.Common.Models;
using MiniDesk.Application.Tickets.Dtos;

namespace MiniDesk.Application.Tickets;

//read side && DTOs only && AsNoTracking
public interface ITicketQueries
{
    Task<TicketDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<PagedResult<TicketListItemDto>> GetPagedAsync(TicketFilter filter, CancellationToken ct);
    Task<CommentDto?> GetCommentByIdAsync(int commentId, CancellationToken ct);
}