using MiniDesk.Application.Common.Models;
using MiniDesk.Application.Users.Dtos;

namespace MiniDesk.Application.Users;

public interface IUserService
{
    Task<IReadOnlyList<StaffDto>> GetStaffAsync(CancellationToken ct);
    Task ChangeRoleAsync(Guid userId, ChangeRoleRequest request, CancellationToken ct);
}
