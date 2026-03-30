namespace Cyberius.Application.Features.Blog.Categories.Models;

public record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Color,
    string? IconUrl,
    int PostCount);