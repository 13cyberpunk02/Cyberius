namespace Cyberius.Application.Features.Blog.Tags.Models;

public record TagResponse(
    Guid Id,
    string Name,
    string Slug,
    int PostCount);