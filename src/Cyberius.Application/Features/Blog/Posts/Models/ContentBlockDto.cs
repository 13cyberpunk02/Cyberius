using Cyberius.Domain.Entities.Enums;

namespace Cyberius.Application.Features.Blog.Posts.Models;

public record ContentBlockDto(
    Guid Id,
    BlockType Type,
    int Order,
    string? Content,
    string? Language,
    string? ImageUrl,
    string? ImageCaption,
    string? CalloutType);