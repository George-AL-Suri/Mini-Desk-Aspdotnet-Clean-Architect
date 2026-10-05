using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniDesk.Application.Common.Constants;

namespace MiniDesk.Infrastructure.Identity.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var roleManager = sp.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var options = sp.GetRequiredService<IOptions<SeedAdminOptions>>().Value;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(IdentitySeeder));

        // 1. Roles
        foreach (var role in Roles.All)
        {
            if (await roleManager.RoleExistsAsync(role))
                continue;

            var result = await roleManager.CreateAsync(new ApplicationRole(role));
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to create role '{role}': {Describe(result)}");

            logger.LogInformation("Role {Role} created.", role);
        }

        // 2. Admin user
        if (string.IsNullOrWhiteSpace(options.Email) || string.IsNullOrWhiteSpace(options.Password))
        {
            logger.LogWarning("SeedAdmin:Email or SeedAdmin:Password is not configured. Admin was not seeded.");
            return;
        }

        var admin = await userManager.FindByEmailAsync(options.Email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = options.Email,
                Email = options.Email,
                EmailConfirmed = true
            };

            var create = await userManager.CreateAsync(admin, options.Password);
            if (!create.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to create admin '{options.Email}': {Describe(create)}");

            logger.LogInformation("Admin user {Email} created.", options.Email);
        }

        // 3. Admin in Admin role
        if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
        {
            var add = await userManager.AddToRoleAsync(admin, Roles.Admin);
            if (!add.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to add admin to role: {Describe(add)}");
        }
    }

    private static string Describe(IdentityResult result) =>
        string.Join(", ", result.Errors.Select(e => e.Description));
}