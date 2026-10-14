using Microsoft.AspNetCore.Identity;
using MiniDesk.Application.Common.Constants;
using MiniDesk.Application.Common.Interfaces;
using MiniDesk.Application.Common.Models;

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

    public async Task<IReadOnlyList<StaffDto>> GetStaffAsync(CancellationToken ct)
    {
        var admins = await userManager.GetUsersInRoleAsync(Roles.Admin);
        var agents = await userManager.GetUsersInRoleAsync(Roles.Agent);

        return admins.Concat(agents)
            .DistinctBy(u => u.Id)
            .OrderBy(u => u.UserName)
            .Select(u => new StaffDto(u.Id, u.UserName!))
            .ToList();
    }
}