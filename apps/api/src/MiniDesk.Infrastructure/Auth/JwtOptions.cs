using System.ComponentModel.DataAnnotations;

namespace MiniDesk.Infrastructure.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    [Required] public string Issuer { get; set; } = default!;
    [Required] public string Audience { get; set; } = default!;
    [Required, MinLength(32)] public string Key { get; set; } = default!;
    [Range(1, 1440)] public int AccessTokenMinutes { get; set; } = 15;
    [Range(1, 90)] public int RefreshTokenDays { get; set; } = 7;
}