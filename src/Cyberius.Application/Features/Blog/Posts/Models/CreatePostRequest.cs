namespace Cyberius.Application.Features.Blog.Posts.Models;

public record CreatePostRequest(
    string Title,
    string? Excerpt,
    string? CoverImageUrl,
    Guid CategoryId,
    List<string> Tags,
    List<CreateContentBlockRequest> Blocks);