using MiniDesk.Domain.Enums;
namespace MiniDesk.Application.Tickets.Dtos;

public record CreateTicketRequest(
    string Title,
    string Description,
    TicketPriority Priority,
    int CategoryId);