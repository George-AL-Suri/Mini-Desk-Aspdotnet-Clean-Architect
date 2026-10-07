namespace MiniDesk.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<bool> IsStaffAsync(Guid userId, CancellationToken ct);
}
