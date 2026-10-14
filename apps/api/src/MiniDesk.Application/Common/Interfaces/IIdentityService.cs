using MiniDesk.Application.Common.Models;

namespace MiniDesk.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<bool> IsStaffAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<StaffDto>> GetStaffAsync(CancellationToken ct);
}
