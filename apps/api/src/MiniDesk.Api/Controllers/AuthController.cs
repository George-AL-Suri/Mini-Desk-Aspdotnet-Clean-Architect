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
    [AllowAnonymous, HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest r, CancellationToken ct) =>
        Ok(await auth.RegisterAsync(r, ct));

    [AllowAnonymous, HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest r, CancellationToken ct) =>
        Ok(await auth.LoginAsync(r, ct));

    [AllowAnonymous, HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest r, CancellationToken ct) =>
        Ok(await auth.RefreshAsync(r, ct));

    [AllowAnonymous, HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest r, CancellationToken ct)
    {
        await auth.LogoutAsync(r, ct);
        return NoContent();
    }

    [Authorize, HttpGet("me")]
    public IActionResult Me([FromServices] ICurrentUser user) =>
        Ok(new { user.UserId, user.UserName, IsAdmin = user.IsInRole(Roles.Admin) });
}