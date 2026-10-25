using FluentValidation;
using MiniDesk.Application.Common.Constants;
using MiniDesk.Application.Common.Exceptions;
using MiniDesk.Application.Common.Interfaces;
using MiniDesk.Application.Common.Models;
using MiniDesk.Application.Users;
using MiniDesk.Application.Users.Dtos;

public class UserService(
    IIdentityService identity,
    ICurrentUser currentUser,
    IValidator<ChangeRoleRequest> validator) : IUserService
{
    public Task<IReadOnlyList<StaffDto>> GetStaffAsync(CancellationToken ct) =>
        identity.GetStaffAsync(ct);

    public async Task ChangeRoleAsync(Guid userId, ChangeRoleRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        if (!currentUser.IsInRole(Roles.Admin))
            throw new ForbiddenException();

        if (userId == currentUser.UserId)
            throw new ConflictException("You cannot change your own role.");

        await identity.SetRoleAsync(userId, request.Role, ct);
    }
}