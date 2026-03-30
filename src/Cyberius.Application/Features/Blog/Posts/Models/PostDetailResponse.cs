using Cyberius.Domain.Entities.Enums;

namespace Cyberius.Application.Features.Blog.Posts.Models;

public record PostDetailResponse(
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
    List<ContentBlockDto> Blocks,
    Dictionary<string, int> Reactions,
    string? MyReaction,
    int ViewCount,
    int CommentCount);