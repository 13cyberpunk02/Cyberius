namespace Cyberius.Application.Features.Blog.Categories.Models;

public record CreateCategoryRequest(
    string Name,
    string Slug,
    string? Color,
    string? IconUrl);