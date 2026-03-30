using Cyberius.Domain.Entities.Enums;

namespace Cyberius.Application.Features.Blog.Posts.Models;

public record PostSummaryResponse(
    Guid Id,
    string Title,
    string Slug,
    string? Excerpt,
    string? CoverImageUrl,
    int ReadTimeMinutes,
    PostStatus Status,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    AuthorDto Author,
    CategoryDto Category,
    List<string> Tags,
    int ViewCount,
    int CommentCount,
    Dictionary<string, int> Reactions);