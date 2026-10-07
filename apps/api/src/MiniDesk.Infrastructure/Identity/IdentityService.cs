using Microsoft.AspNetCore.Identity;
using MiniDesk.Application.Common.Constants;
using MiniDesk.Application.Common.Interfaces;

namespace MiniDesk.Infrastructure.Identity;

internal class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<bool> IsStaffAsync(Guid userId, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return false;

        var roles = await userManager.GetRolesAsync(user);
        return roles.Contains(Roles.Admin) || roles.Contains(Roles.Agent);
    }
}