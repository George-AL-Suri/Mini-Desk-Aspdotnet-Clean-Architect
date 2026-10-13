// Repositories/CategoryRepository.cs
using Microsoft.EntityFrameworkCore;
using MiniDesk.Application.Categories;
using MiniDesk.Domain.Entities;

namespace MiniDesk.Infrastructure.Persistence.Repositories;

internal class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);   // tracked on purpose

    public Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct) =>
        db.Categories.AnyAsync(c => c.Name == name && (excludeId == null || c.Id != excludeId), ct);

    public Task<bool> HasTicketsAsync(int id, CancellationToken ct) =>
        db.Tickets.AnyAsync(t => t.CategoryId == id, ct);

    public async Task AddAsync(Category category, CancellationToken ct) =>
        await db.Categories.AddAsync(category, ct);

    public void Remove(Category category) => db.Categories.Remove(category);
}