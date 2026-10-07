// Application/Categories/CategoryMapper.cs
using Riok.Mapperly.Abstractions;
using MiniDesk.Domain.Entities;
using MiniDesk.Application.Categories.Dtos;

namespace MiniDesk.Application.Categories;

[Mapper]
public static partial class CategoryMapper
{
    [MapperIgnoreTarget(nameof(Category.Tickets))]
    public static partial CategoryDto ToDto(this Category category);

    // for EF queries: translated to SQL, selects only the DTO columns
    public static partial IQueryable<CategoryDto> ProjectToDto(this IQueryable<Category> query);
}