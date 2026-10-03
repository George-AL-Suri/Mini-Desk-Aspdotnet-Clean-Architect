using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniDesk.Domain.Entities;

namespace MiniDesk.Infrastructure.Persistence.Configurations;

internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.HasKey(k => k.Id);

        b.Property(p => p.Name).IsRequired().HasMaxLength(100);

        b.HasIndex(c => c.Name).IsUnique();
    }
}
