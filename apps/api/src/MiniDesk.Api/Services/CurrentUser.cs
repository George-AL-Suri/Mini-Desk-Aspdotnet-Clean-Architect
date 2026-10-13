using MiniDesk.Application.Common.Interfaces;
using System.Security.Claims;

namespace MiniDesk.Api.Services;

internal class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public string? UserName => Principal?.FindFirstValue(ClaimTypes.Name);

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
}