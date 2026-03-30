namespace Cyberius.Application.Features.Blog.Categories.Models;

public record UpdateCategoryRequest(
    string Name,
    string Slug,
    string? Color,
    string? IconUrl);