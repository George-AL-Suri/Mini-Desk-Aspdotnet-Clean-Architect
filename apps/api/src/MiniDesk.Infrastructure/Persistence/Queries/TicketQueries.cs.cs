using Microsoft.EntityFrameworkCore;
using MiniDesk.Application.Comments.Dtos;
using MiniDesk.Application.Common.Models;
using MiniDesk.Application.Tickets;
using MiniDesk.Application.Tickets.Dtos;
using MiniDesk.Domain.Entities;
using MiniDesk.Infrastructure.Identity;

namespace MiniDesk.Infrastructure.Persistence.Queries;

internal class TicketQueries(AppDbContext db) : ITicketQueries
{
    // ticket + its two users, used by both queries
    private sealed class TicketRow
    {
        public Ticket Ticket { get; init; } = default!;
        public ApplicationUser Creator { get; init; } = default!;
        public ApplicationUser? Assignee { get; init; }
    }

    private IQueryable<TicketRow> JoinUsers(IQueryable<Ticket> tickets) =>
        tickets
            // inner join: every ticket has a creator
            .Join(db.Users,
                t => t.CreatedById,
                u => u.Id,
                (t, creator) => new { t, creator })
            // left join: AssignedToId can be null
            .GroupJoin(db.Users,
                x => x.t.AssignedToId,
                u => (Guid?)u.Id,                       // same type as AssignedToId
                (x, assignees) => new { x.t, x.creator, assignees })
            .SelectMany(
                x => x.assignees.DefaultIfEmpty(),
                (x, assignee) => new TicketRow
                {
                    Ticket = x.t,
                    Creator = x.creator,
                    Assignee = assignee
                });

    // ---------- Get by id (with comments) ----------
    public async Task<TicketDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var row = await JoinUsers(db.Tickets.AsNoTracking().Where(t => t.Id == id))
            .Select(r => new
            {
                r.Ticket.Id,
                r.Ticket.Title,
                r.Ticket.Description,
                r.Ticket.Priority,
                r.Ticket.Status,
                r.Ticket.CategoryId,
                CategoryName = r.Ticket.Category.Name,
                r.Ticket.CreatedById,
                CreatedByName = r.Creator.UserName!,
                r.Ticket.AssignedToId,
                AssignedToName = r.Assignee != null ? r.Assignee.UserName : null,
                r.Ticket.CreatedAt,
                r.Ticket.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (row is null)
            return null;

        var comments = await db.Comments.AsNoTracking()
            .Where(c => c.TicketId == id)
            .Join(db.Users,
                c => c.AuthorId,
                u => u.Id,
                (c, author) => new { c, author })
            .OrderBy(x => x.c.CreatedAt).ThenBy(x => x.c.Id)
            .Select(x => new CommentDto(
                x.c.Id,
                x.c.TicketId,
                x.c.Content,
                x.c.AuthorId,
                x.author.UserName!,
                x.c.CreatedAt))
            .ToListAsync(ct);

        return new TicketDto(
            row.Id, row.Title, row.Description, row.Priority, row.Status,
            row.CategoryId, row.CategoryName,
            row.CreatedById, row.CreatedByName,
            row.AssignedToId, row.AssignedToName,
            row.CreatedAt, row.UpdatedAt,
            comments);
    }

    // ---------- Paged list ----------
    public async Task<PagedResult<TicketListItemDto>> GetPagedAsync(TicketFilter filter, CancellationToken ct)
    {
        // 1. filter on Ticket columns
        var tickets = db.Tickets.AsNoTracking().AsQueryable();

        if (filter.Status is not null)
            tickets = tickets.Where(t => t.Status == filter.Status);

        if (filter.Priority is not null)
            tickets = tickets.Where(t => t.Priority == filter.Priority);

        if (filter.CategoryId is not null)
            tickets = tickets.Where(t => t.CategoryId == filter.CategoryId);

        if (filter.AssignedToId is not null)
            tickets = tickets.Where(t => t.AssignedToId == filter.AssignedToId);

        if (filter.CreatedById is not null)
            tickets = tickets.Where(t => t.CreatedById == filter.CreatedById);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            tickets = tickets.Where(t => t.Title.Contains(search));
        }

        // 2. count before joining and paging (the creator join never removes rows)
        var totalCount = await tickets.CountAsync(ct);

        // 3. join, sort on the joined result, then page
        var rows = JoinUsers(tickets);

        rows = (filter.SortBy, filter.Descending) switch
        {
            (TicketSortBy.Priority, true) => rows.OrderByDescending(r => r.Ticket.Priority).ThenByDescending(r => r.Ticket.Id),
            (TicketSortBy.Priority, false) => rows.OrderBy(r => r.Ticket.Priority).ThenBy(r => r.Ticket.Id),
            (TicketSortBy.Status, true) => rows.OrderByDescending(r => r.Ticket.Status).ThenByDescending(r => r.Ticket.Id),
            (TicketSortBy.Status, false) => rows.OrderBy(r => r.Ticket.Status).ThenBy(r => r.Ticket.Id),
            (TicketSortBy.Title, true) => rows.OrderByDescending(r => r.Ticket.Title).ThenByDescending(r => r.Ticket.Id),
            (TicketSortBy.Title, false) => rows.OrderBy(r => r.Ticket.Title).ThenBy(r => r.Ticket.Id),
            (_, true) => rows.OrderByDescending(r => r.Ticket.CreatedAt).ThenByDescending(r => r.Ticket.Id),
            _ => rows.OrderBy(r => r.Ticket.CreatedAt).ThenBy(r => r.Ticket.Id)
        };

        var items = await rows
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new TicketListItemDto(
                r.Ticket.Id,
                r.Ticket.Title,
                r.Ticket.Priority,
                r.Ticket.Status,
                r.Ticket.CategoryId,
                r.Ticket.Category.Name,
                r.Creator.UserName!,
                r.Assignee != null ? r.Assignee.UserName : null,
                r.Ticket.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<TicketListItemDto>(items, filter.Page, filter.PageSize, totalCount);
    }

    // ---------- Single comment ----------
    public Task<CommentDto?> GetCommentByIdAsync(int commentId, CancellationToken ct) =>
        db.Comments.AsNoTracking()
            .Where(c => c.Id == commentId)
            .Join(db.Users,
                c => c.AuthorId,
                u => u.Id,
                (c, author) => new CommentDto(
                    c.Id,
                    c.TicketId,
                    c.Content,
                    c.AuthorId,
                    author.UserName!,
                    c.CreatedAt))
            .FirstOrDefaultAsync(ct);
}