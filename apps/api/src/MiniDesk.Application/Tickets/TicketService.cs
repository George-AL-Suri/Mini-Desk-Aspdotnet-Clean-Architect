using FluentValidation;
using MiniDesk.Application.Comments.Dtos;
using MiniDesk.Application.Comments.Mappers;
using MiniDesk.Application.Common.Constants;
using MiniDesk.Application.Common.Exceptions;
using MiniDesk.Application.Common.Interfaces;
using MiniDesk.Application.Common.Models;
using MiniDesk.Application.Tickets.Dtos;
using MiniDesk.Application.Tickets.Mappers;
using MiniDesk.Domain.Entities;
using MiniDesk.Domain.Enums;

namespace MiniDesk.Application.Tickets;

public class TicketService(
    ITicketRepository tickets,
    ITicketQueries queries,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IIdentityService identity,
    IValidator<CreateTicketRequest> createValidator,
    IValidator<UpdateTicketRequest> updateValidator,
    IValidator<ChangeTicketStatusRequest> statusValidator,
    IValidator<AssignTicketRequest> assignValidator,
    IValidator<CreateCommentRequest> commentValidator,
    IValidator<TicketFilter> filterValidator) : ITicketService
{
    // ---------- Create ----------
    public async Task<TicketDto> CreateAsync(CreateTicketRequest request, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        var categoryName = await tickets.GetCategoryNameAsync(request.CategoryId, ct)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        var ticket = request.ToEntity();
        ticket.CreatedById = CurrentUserId;
        ticket.Status = TicketStatus.Open;

        await tickets.AddAsync(ticket, ct);
        await unitOfWork.SaveChangesAsync(ct);          // ticket.Id and CreatedAt are set now

        return new TicketDto(
            ticket.Id, ticket.Title, ticket.Description,
            ticket.Priority, ticket.Status,
            ticket.CategoryId, categoryName,
            ticket.CreatedById, currentUser.UserName ?? "",
            null, null,                                  // not assigned yet
            ticket.CreatedAt, ticket.UpdatedAt,
            [],                                          // no comments yet
            Convert.ToBase64String(ticket.RowVersion));                                         
    }

    // ---------- Get all ----------
    public async Task<PagedResult<TicketListItemDto>> GetAllAsync(TicketFilter filter, CancellationToken ct)
    {
        await filterValidator.ValidateAndThrowAsync(filter, ct);

        // plain users can only see their own tickets, whatever the client sent
        if (!IsStaff)
            filter = filter with { CreatedById = CurrentUserId };

        return await queries.GetPagedAsync(filter, ct);
    }

    // ---------- Get by id ----------
    public async Task<TicketDto> GetByIdAsync(int id, CancellationToken ct)
    {
        var dto = await queries.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Ticket), id);

        if (!CanSee(dto.CreatedById, dto.AssignedToId))
            throw new ForbiddenException();

        return dto;
    }

    // ---------- Update ----------
    public async Task UpdateAsync(int id, UpdateTicketRequest request, CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);

        var ticket = await LoadAsync(id, ct);

        if (ticket.Status == TicketStatus.Closed)
            throw new ConflictException("A closed ticket cannot be edited.");

        var isCreatorWhileOpen = ticket.CreatedById == CurrentUserId && ticket.Status == TicketStatus.Open;
        if (!IsStaff && !isCreatorWhileOpen)
            throw new ForbiddenException();

        if (request.CategoryId != ticket.CategoryId &&
            !await tickets.CategoryExistsAsync(request.CategoryId, ct))
            throw new NotFoundException(nameof(Category), request.CategoryId);

        tickets.SetRowVersion(ticket, Convert.FromBase64String(request.RowVersion));
        request.Apply(ticket);                    // Mapperly updates the tracked entity
        await unitOfWork.SaveChangesAsync(ct);
    }

    // ---------- Change status ----------
    public async Task ChangeStatusAsync(int id, ChangeTicketStatusRequest request, CancellationToken ct)
    {
        await statusValidator.ValidateAndThrowAsync(request, ct);

        var ticket = await LoadAsync(id, ct);

        var isAssignee = ticket.AssignedToId == CurrentUserId;
        if (!currentUser.IsInRole(Roles.Admin) && !isAssignee)
            throw new ForbiddenException();

        if (!IsValidTransition(ticket.Status, request.Status))
            throw new ConflictException($"Cannot change status from {ticket.Status} to {request.Status}.");

        ticket.Status = request.Status;
        await unitOfWork.SaveChangesAsync(ct);
    }

    // ---------- Assign ----------
    public async Task AssignAsync(int id, AssignTicketRequest request, CancellationToken ct)
    {
        await assignValidator.ValidateAndThrowAsync(request, ct);

        if (!IsStaff)
            throw new ForbiddenException();

        var ticket = await LoadAsync(id, ct);

        if (ticket.Status == TicketStatus.Closed)
            throw new ConflictException("A closed ticket cannot be assigned.");

        if (!await identity.IsStaffAsync(request.AssignedToId, ct))
            throw new ConflictException("The ticket can only be assigned to an agent or an admin.");

        ticket.AssignedToId = request.AssignedToId;
        if (ticket.Status == TicketStatus.Open)
            ticket.Status = TicketStatus.InProgress;   // optional rule, remove if you don't want it

        await unitOfWork.SaveChangesAsync(ct);
    }

    // ---------- Add comment ----------
    public async Task<CommentDto> AddCommentAsync(int ticketId, CreateCommentRequest request, CancellationToken ct)
    {
        await commentValidator.ValidateAndThrowAsync(request, ct);

        var ticket = await LoadAsync(ticketId, ct);

        if (!CanSee(ticket.CreatedById, ticket.AssignedToId))
            throw new ForbiddenException();

        if (ticket.Status == TicketStatus.Closed)
            throw new ConflictException("You cannot comment on a closed ticket.");

        var comment = request.ToEntity();          // Mapperly
        comment.TicketId = ticketId;
        comment.AuthorId = CurrentUserId;

        await tickets.AddCommentAsync(comment, ct);
        await unitOfWork.SaveChangesAsync(ct);     // comment.Id and CreatedAt are set now

        return new CommentDto(
            comment.Id,
            comment.TicketId,
            comment.Content,
            comment.AuthorId,
            currentUser.UserName ?? "",            // author is the current user
            comment.CreatedAt);
    }


    #region ---------- Helpers ----------
    private Guid CurrentUserId => currentUser.UserId ?? throw new ForbiddenException();

    private bool IsStaff => currentUser.IsInRole(Roles.Admin) || currentUser.IsInRole(Roles.Agent);

    private bool CanSee(Guid createdById, Guid? assignedToId) =>
        IsStaff || createdById == currentUser.UserId || assignedToId == currentUser.UserId;

    private async Task<Ticket> LoadAsync(int id, CancellationToken ct) =>
        await tickets.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Ticket), id);

    private static bool IsValidTransition(TicketStatus from, TicketStatus to) => (from, to) switch
    {
        (TicketStatus.Open, TicketStatus.InProgress) => true,
        (TicketStatus.InProgress, TicketStatus.Resolved) => true,
        (TicketStatus.InProgress, TicketStatus.Open) => true,
        (TicketStatus.Resolved, TicketStatus.Closed) => true,
        (TicketStatus.Resolved, TicketStatus.InProgress) => true,
        _ => false
    }; 
    #endregion
}