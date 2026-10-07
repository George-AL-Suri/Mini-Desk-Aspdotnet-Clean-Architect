using MiniDesk.Domain.Enums;
namespace MiniDesk.Application.Tickets.Dtos;

public record TicketListItemDto(
    int Id,
    string Title,
    TicketPriority Priority,
    TicketStatus Status,
    int CategoryId,
    string CategoryName,
    string CreatedByName,
    string? AssignedToName,
    DateTime CreatedAt);