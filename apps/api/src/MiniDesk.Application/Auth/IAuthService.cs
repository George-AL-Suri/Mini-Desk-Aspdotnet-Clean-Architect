using MiniDesk.Application.Auth.Dtos;

namespace MiniDesk.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct);

}
