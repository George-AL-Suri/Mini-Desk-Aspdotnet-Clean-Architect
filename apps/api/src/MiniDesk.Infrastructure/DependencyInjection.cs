using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniDesk.Application.Auth;
using MiniDesk.Application.Categories;
using MiniDesk.Application.Comments;
using MiniDesk.Application.Common.Interfaces;
using MiniDesk.Application.Tickets;
using MiniDesk.Infrastructure.Auth;
using MiniDesk.Infrastructure.Identity;
using MiniDesk.Infrastructure.Identity.Seed;
using MiniDesk.Infrastructure.Persistence;
using MiniDesk.Infrastructure.Persistence.Queries;
using MiniDesk.Infrastructure.Persistence.Repositories;

namespace MiniDesk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        //services.Configure<SeedAdminOptions>(config.GetSection(SeedAdminOptions.SectionName));
        //services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));
        services.AddOptions<JwtOptions>()
                .Bind(config.GetSection(JwtOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.AddOptions<SeedAdminOptions>()
                .Bind(config.GetSection(SeedAdminOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                config.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddIdentityCore<ApplicationUser>(o =>
        {
            o.Password.RequiredLength = 8;
            o.User.RequireUniqueEmail = true;
        })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        // Infrastructure/DependencyInjection.cs
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketQueries, TicketQueries>();

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryQueries, CategoryQueries>();

        services.AddScoped<ICommentRepository, CommentRepository>();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IAuthService, AuthService>();


        return services;
    }
}
