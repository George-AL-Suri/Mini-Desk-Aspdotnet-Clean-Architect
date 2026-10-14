using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniDesk.Infrastructure.Auth;
using MiniDesk.Infrastructure.Identity;

namespace MiniDesk.Infrastructure.Persistence.Configurations;

internal class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.HasKey(t => t.Id);
        b.Property(t => t.TokenHash).IsRequired().HasMaxLength(64);   // SHA-256 hex
        b.HasIndex(t => t.TokenHash).IsUnique();
        b.HasIndex(t => t.UserId);

        b.HasOne<ApplicationUser>().WithMany()
         .HasForeignKey(t => t.UserId)
         .OnDelete(DeleteBehavior.Cascade);   // only cascade path to AspNetUsers from this table
    }
}
