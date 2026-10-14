namespace MiniDesk.Infrastructure.Auth;

public class RefreshToken
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
