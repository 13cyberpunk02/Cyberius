namespace Cyberius.Application.Features.Blog.Comments.Models;

public record CommentAuthorDto(
    Guid Id,
    string Username,
    string FullName,
    string? AvatarUrl);