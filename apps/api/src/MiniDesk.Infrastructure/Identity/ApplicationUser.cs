using Microsoft.AspNetCore.Identity;

namespace MiniDesk.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; }
}
