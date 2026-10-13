using MiniDesk.Domain.Entities;

namespace MiniDesk.Application.Categories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id, CancellationToken ct);   // tracked
    Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct);
    Task<bool> HasTicketsAsync(int id, CancellationToken ct);
    Task AddAsync(Category category, CancellationToken ct);
    void Remove(Category category);
}
