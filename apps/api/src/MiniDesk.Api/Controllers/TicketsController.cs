using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniDesk.Application.Comments.Dtos;
using MiniDesk.Application.Common.Constants;
using MiniDesk.Application.Common.Models;
using MiniDesk.Application.Tickets;
using MiniDesk.Application.Tickets.Dtos;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketsController(ITicketService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<TicketListItemDto>>> GetAll(
        [FromQuery] TicketFilter filter, CancellationToken ct) =>
        Ok(await service.GetAllAsync(filter, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketDto>> GetById(int id, CancellationToken ct) =>
        Ok(await service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<TicketDto>> Create(CreateTicketRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateTicketRequest request, CancellationToken ct)
    {
        await service.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, ChangeTicketStatusRequest request, CancellationToken ct)
    {
        await service.ChangeStatusAsync(id, request, ct);
        return NoContent();
    }

    [HttpPut("{id:int}/assignee")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Agent}")]
    public async Task<IActionResult> Assign(int id, AssignTicketRequest request, CancellationToken ct)
    {
        await service.AssignAsync(id, request, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(int id, CreateCommentRequest request, CancellationToken ct) =>
        Ok(await service.AddCommentAsync(id, request, ct));
}