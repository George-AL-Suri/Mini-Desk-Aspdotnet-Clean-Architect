using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniDesk.Application.Auth;
using MiniDesk.Application.Auth.Dtos;
using MiniDesk.Application.Common.Constants;
using MiniDesk.Application.Common.Interfaces;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct) =>
        Ok(await auth.LoginAsync(request, ct));

    // quick check that the token and ICurrentUser work
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me([FromServices] ICurrentUser user) =>
        Ok(new { user.UserId, user.UserName, IsAdmin = user.IsInRole(Roles.Admin) });
}