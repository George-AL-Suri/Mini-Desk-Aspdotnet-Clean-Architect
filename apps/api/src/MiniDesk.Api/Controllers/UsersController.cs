using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniDesk.Application.Common.Constants;
using MiniDesk.Application.Common.Interfaces;
using MiniDesk.Application.Common.Models;
using MiniDesk.Application.Users;
using MiniDesk.Application.Users.Dtos;

namespace MiniDesk.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Agent}")]
public class UsersController(IUserService users) : ControllerBase
{
    [HttpGet("staff")]
    public async Task<ActionResult<IReadOnlyList<StaffDto>>> GetStaff(CancellationToken ct) =>
        Ok(await users.GetStaffAsync(ct));

    [HttpPut("{id:guid}/role")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> ChangeRole(Guid id, ChangeRoleRequest request, CancellationToken ct)
    {
        await users.ChangeRoleAsync(id, request, ct);
        return NoContent();
    }
}