using MiniDesk.Domain.Enums;
namespace MiniDesk.Application.Tickets.Dtos;

public record TicketFilter
{
    public string? Search { get; init; }
    public TicketStatus? Status { get; init; }
    public TicketPriority? Priority { get; init; }
    public int? CategoryId { get; init; }
    public Guid? AssignedToId { get; init; }
    public Guid? CreatedById { get; init; }   // the service forces this for plain users

    public TicketSortBy SortBy { get; init; } = TicketSortBy.CreatedAt;
    public bool Descending { get; init; } = true;

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}