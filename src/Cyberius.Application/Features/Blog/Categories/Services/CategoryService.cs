using Cyberius.Application.Features.Blog.Categories.Models;
using Cyberius.Application.Features.Blog.Interfaces;
using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Domain.Shared;

namespace Cyberius.Application.Features.Blog.Categories.Services;

public sealed class CategoryService(
    IUnitOfWork uow) : ICategoryService
{
        public async Task<Result<List<CategoryResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var result = await uow.Categories.GetWithPostCountAsync(ct);
        return result
            .Select(x => MapResponse(x.Category, x.PostCount))
            .ToList();
    }
 
    public async Task<Result<CategoryResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var category = await uow.Categories.GetByIdAsync(id, ct);
        if (category is null)
            return Errors.Category.NotFound(id.ToString());
 
        return MapResponse(category, postCount: 0);
    }
 
    public async Task<Result<CategoryResponse>> CreateAsync(
        CreateCategoryRequest request, CancellationToken ct = default)
    {
        if (await uow.Categories.SlugExistsAsync(request.Slug, ct: ct))
            return Errors.Category.SlugAlreadyExists(request.Slug);
 
        var category = new Category
        {
            Id      = Guid.NewGuid(),
            Name    = request.Name.Trim(),
            Slug    = request.Slug.Trim().ToLower(),
            Color   = request.Color,
            IconUrl = request.IconUrl,
        };
 
        await uow.Categories.AddAsync(category, ct);
        await uow.SaveChangesAsync(ct);
 
        return MapResponse(category, postCount: 0);
    }
 
    public async Task<Result<CategoryResponse>> UpdateAsync(
        Guid id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await uow.Categories.GetByIdAsync(id, ct);
        if (category is null)
            return Errors.Category.NotFound(id.ToString());
 
        if (await uow.Categories.SlugExistsAsync(request.Slug, excludeId: id, ct: ct))
            return Errors.Category.SlugAlreadyExists(request.Slug);
 
        category.Name    = request.Name.Trim();
        category.Slug    = request.Slug.Trim().ToLower();
        category.Color   = request.Color;
        category.IconUrl = request.IconUrl;
 
        uow.Categories.Update(category);
        await uow.SaveChangesAsync(ct);
 
        return MapResponse(category, postCount: 0);
    }
 
    public async Task<Result<string>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await uow.Categories.GetByIdAsync(id, ct);
        if (category is null)
            return Errors.Category.NotFound(id.ToString());
 
        // Нельзя удалить категорию с существующими статьями
        var hasPosts = (await uow.Posts.GetByCategoryAsync(id, 1, 1, ct)).TotalCount > 0;
        if (hasPosts)
            return Errors.Category.HasPosts();
 
        uow.Categories.Remove(category);
        await uow.SaveChangesAsync(ct);
        return Result<string>.Success("Успешно удалено");
    }
 
    private static CategoryResponse MapResponse(Category c, int postCount) =>
        new(c.Id, c.Name, c.Slug, c.Color, c.IconUrl, postCount);
}