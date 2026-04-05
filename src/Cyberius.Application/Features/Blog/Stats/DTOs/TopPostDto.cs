namespace Cyberius.Application.Features.Blog.Stats.DTOs;

public record TopPostDto(
    Guid   Id,
    string Title,
    string Slug,
    int    ViewCount,
    int    CommentCount
);