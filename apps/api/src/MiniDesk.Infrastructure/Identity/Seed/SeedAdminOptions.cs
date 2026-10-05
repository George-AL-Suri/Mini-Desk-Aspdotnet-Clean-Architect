namespace MiniDesk.Infrastructure.Identity.Seed;

public class SeedAdminOptions
{
    public const string SectionName = "SeedAdmin";

    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FullName { get; set; } = "System Admin";
}