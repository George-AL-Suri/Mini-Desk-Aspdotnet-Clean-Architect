using MiniDesk.Domain.Enums;
namespace MiniDesk.Application.Tickets.Dtos;

public record UpdateTicketRequest(
    string Title,
    string Description,
    TicketPriority Priority,
    int CategoryId);