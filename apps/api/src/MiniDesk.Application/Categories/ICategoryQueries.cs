using MiniDesk.Application.Categories.Dtos;

namespace MiniDesk.Application.Categories;

public interface ICategoryQueries
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct);
    Task<CategoryDto?> GetByIdAsync(int id, CancellationToken ct);
}
