namespace Cyberius.Application.Features.Blog.Comments.Models;

public record CommentResponse(
    Guid Id,
    string Content,
    bool IsEdited,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    CommentAuthorDto Author,
    Dictionary<string, int> Reactions,
    string? MyReaction,
    int ReplyCount,
    List<CommentResponse>? Replies);