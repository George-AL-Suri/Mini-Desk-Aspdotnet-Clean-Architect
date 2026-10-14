using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniDesk.Application.Common.Interfaces;
using MiniDesk.Domain.Entities;
using MiniDesk.Infrastructure.Auth;
using MiniDesk.Infrastructure.Identity;

namespace MiniDesk.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected AppDbContext()
    {
    }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Comment> Comments => Set<Comment>(); 
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}


/*
 * dotnet ef migrations add InitialCreate --project src/MiniDesk.Infrastructure --startup-project src/MiniDesk.Api --output-dir Persistence/Migrations
 * dotnet ef database update --project src/MiniDesk.Infrastructure --startup-project src/MiniDesk.Api
 * dotnet ef migrations remove  --project src/MiniDesk.Infrastructure --startup-project src/MiniDesk.Api
 * dotnet ef migrations list    --project src/MiniDesk.Infrastructure --startup-project src/MiniDesk.Api
 * dotnet ef migrations script  --project src/MiniDesk.Infrastructure --startup-project src/MiniDesk.Api -o migrate.sql
 * 
 * dotnet ef migrations add AddRefreshTokens --project src/MiniDesk.Infrastructure --startup-project src/MiniDesk.Api --output-dir Persistence/Migrations
 */