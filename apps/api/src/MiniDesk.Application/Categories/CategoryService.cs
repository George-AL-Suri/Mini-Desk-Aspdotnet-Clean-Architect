// CategoryService.cs
using FluentValidation;
using MiniDesk.Application.Categories.Dtos;
using MiniDesk.Application.Categories.Mappers;
using MiniDesk.Application.Common.Exceptions;
using MiniDesk.Application.Common.Interfaces;
using MiniDesk.Domain.Entities;

namespace MiniDesk.Application.Categories;

public class CategoryService(
    ICategoryRepository categories,
    ICategoryQueries queries,
    IUnitOfWork unitOfWork,
    IValidator<CreateCategoryRequest> createValidator,
    IValidator<UpdateCategoryRequest> updateValidator) : ICategoryService
{
    public Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct) =>
        queries.GetAllAsync(ct);

    public async Task<CategoryDto> GetByIdAsync(int id, CancellationToken ct) =>
        await queries.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Category), id);

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        var name = request.Name.Trim();
        if (await categories.NameExistsAsync(name, null, ct))
            throw new ConflictException($"A category named '{name}' already exists.");

        var category = (request with { Name = name }).ToEntity();   // Mapperly
        await categories.AddAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return category.ToDto();                                    // Id is filled after save
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);

        var category = await LoadAsync(id, ct);

        var name = request.Name.Trim();
        if (await categories.NameExistsAsync(name, id, ct))
            throw new ConflictException($"A category named '{name}' already exists.");

        category.Name = name;
        await unitOfWork.SaveChangesAsync(ct);

        return category.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var category = await LoadAsync(id, ct);

        if (await categories.HasTicketsAsync(id, ct))
            throw new ConflictException("This category is used by tickets and cannot be deleted.");

        categories.Remove(category);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Category> LoadAsync(int id, CancellationToken ct) =>
        await categories.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Category), id);
}