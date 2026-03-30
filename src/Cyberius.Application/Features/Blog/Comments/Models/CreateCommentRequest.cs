namespace Cyberius.Application.Features.Blog.Comments.Models;

public record CreateCommentRequest(
    Guid PostId,
    string Content,
    Guid? ParentCommentId);