using MiniDesk.Application.Comments.Dtos;
using MiniDesk.Domain.Enums;
namespace MiniDesk.Application.Tickets.Dtos;

public record TicketDto(
    int Id,
    string Title,
    string Description,
    TicketPriority Priority,
    TicketStatus Status,
    int CategoryId,
    string CategoryName,
    Guid CreatedById,
    string CreatedByName,
    Guid? AssignedToId,
    string? AssignedToName,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<CommentDto> Comments);