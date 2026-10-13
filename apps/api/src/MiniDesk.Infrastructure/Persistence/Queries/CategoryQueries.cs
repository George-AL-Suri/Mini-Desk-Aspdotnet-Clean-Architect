using Microsoft.EntityFrameworkCore;
using MiniDesk.Application.Categories.Mappers;
using MiniDesk.Application.Categories.Dtos;
using MiniDesk.Application.Categories;

namespace MiniDesk.Infrastructure.Persistence.Queries;

internal class CategoryQueries(AppDbContext db) : ICategoryQueries
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct) =>
        await db.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .ProjectToDto()                      
            .ToListAsync(ct);

    public Task<CategoryDto?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Categories.AsNoTracking()
            .Where(c => c.Id == id)
            .ProjectToDto()
            .FirstOrDefaultAsync(ct);
}