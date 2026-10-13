// Application/Categories/CategoryMapper.cs
using MiniDesk.Application.Categories.Dtos;
using MiniDesk.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace MiniDesk.Application.Categories.Mappers;

[Mapper]
public static partial class CategoryMapper
{
    // entity -> DTO (used after create/update)
    public static partial CategoryDto ToDto(this Category category);

    // query projection (translated to SQL: SELECT Id, Name)
    public static partial IQueryable<CategoryDto> ProjectToDto(this IQueryable<Category> query);

    // create request -> new entity
    [MapperIgnoreTarget(nameof(Category.Id))]
    [MapperIgnoreTarget(nameof(Category.Tickets))]
    public static partial Category ToEntity(this CreateCategoryRequest request);
}