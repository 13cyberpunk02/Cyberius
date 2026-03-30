namespace Cyberius.Application.Features.Blog.Posts.Models;

public record AuthorDto(
    Guid Id,
    string Username,
    string FullName,
    string? AvatarUrl);