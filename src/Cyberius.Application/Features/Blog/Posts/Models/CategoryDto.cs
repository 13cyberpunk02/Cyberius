namespace Cyberius.Application.Features.Blog.Posts.Models;

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Color,
    string? IconUrl);