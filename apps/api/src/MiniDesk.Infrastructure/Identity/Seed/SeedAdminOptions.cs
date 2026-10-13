using System.ComponentModel.DataAnnotations;

namespace MiniDesk.Infrastructure.Identity.Seed;

public class SeedAdminOptions
{
    public const string SectionName = "SeedAdmin";
    [Required, EmailAddress] public string Email { get; set; } = default!;
    [Required, MinLength(8)] public string Password { get; set; } = default!;
}