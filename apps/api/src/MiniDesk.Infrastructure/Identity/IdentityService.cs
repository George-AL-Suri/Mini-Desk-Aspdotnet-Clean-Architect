using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using MiniDesk.Application.Common.Constants;
using MiniDesk.Application.Common.Exceptions;
using MiniDesk.Application.Common.Interfaces;
using MiniDesk.Application.Common.Models;
using MiniDesk.Infrastructure.Persistence;

namespace MiniDesk.Infrastructure.Identity;

internal class IdentityService(
    UserManager<ApplicationUser> userManager,
    AppDbContext db) : IIdentityService
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
    public async Task SetRoleAsync(Guid userId, string role, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException(nameof(ApplicationUser), userId);

        var current = await userManager.GetRolesAsync(user);
        if (current.Count == 1 && current[0] == role)
            return;                                       // already has it, nothing to do

        await using var tx = await db.Database.BeginTransactionAsync(ct);   // <- 2. start

        var remove = await userManager.RemoveFromRolesAsync(user, current);
        if (!remove.Succeeded) throw ToValidation(remove);

        var add = await userManager.AddToRoleAsync(user, role);
        if (!add.Succeeded) throw ToValidation(add);

        await tx.CommitAsync(ct);                                           // <- 3. commit
    }

    private static ValidationException ToValidation(IdentityResult result) =>
        new(result.Errors.Select(e => new ValidationFailure(e.Code, e.Description)));
}