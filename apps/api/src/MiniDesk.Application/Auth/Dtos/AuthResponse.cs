namespace MiniDesk.Application.Auth.Dtos;

public record AuthResponse(string AccessToken, DateTime ExpiresAt);
