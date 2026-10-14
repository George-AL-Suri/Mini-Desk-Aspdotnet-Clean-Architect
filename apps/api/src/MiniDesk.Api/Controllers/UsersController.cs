using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniDesk.Application.Common.Constants;
using MiniDesk.Application.Common.Interfaces;
using MiniDesk.Application.Common.Models;

namespace MiniDesk.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Agent}")]
public class UsersController(IIdentityService identity) : ControllerBase
{
    [HttpGet("staff")]
    public async Task<ActionResult<IReadOnlyList<StaffDto>>> GetStaff(CancellationToken ct) =>
        Ok(await identity.GetStaffAsync(ct));
}
