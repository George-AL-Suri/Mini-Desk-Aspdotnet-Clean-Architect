using MiniDesk.Application.Auth.Dtos;

namespace MiniDesk.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken ct);
    Task LogoutAsync(RefreshRequest refreshRequest, CancellationToken ct);

}
