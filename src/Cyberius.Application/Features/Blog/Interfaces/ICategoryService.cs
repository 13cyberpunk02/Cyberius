using Cyberius.Application.Features.Blog.Categories.Models;

namespace Cyberius.Application.Features.Blog.Interfaces;

public interface ICategoryService
{
    Task<Result<List<CategoryResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<CategoryResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<CategoryResponse>> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<Result<CategoryResponse>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default);
    Task<Result<string>> DeleteAsync(Guid id, CancellationToken ct = default);
}