using MiniDesk.Application.Comments.Dtos;
using MiniDesk.Application.Common.Models;
using MiniDesk.Application.Tickets.Dtos;

namespace MiniDesk.Application.Tickets;

//Use Cases
public interface ITicketService
{
    Task<TicketDto> CreateAsync(CreateTicketRequest request, CancellationToken ct);
    Task<PagedResult<TicketListItemDto>> GetAllAsync(TicketFilter filter, CancellationToken ct);
    Task<TicketDto> GetByIdAsync(int id, CancellationToken ct);
    Task UpdateAsync(int id, UpdateTicketRequest request, CancellationToken ct);
    Task ChangeStatusAsync(int id, ChangeTicketStatusRequest request, CancellationToken ct);
    Task AssignAsync(int id, AssignTicketRequest request, CancellationToken ct);
    Task<CommentDto> AddCommentAsync(int ticketId, CreateCommentRequest request, CancellationToken ct);
}