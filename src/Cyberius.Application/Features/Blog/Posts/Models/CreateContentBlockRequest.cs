using Cyberius.Domain.Entities.Enums;

namespace Cyberius.Application.Features.Blog.Posts.Models;

public record CreateContentBlockRequest(
    BlockType Type,
    int Order,
    string? Content,
    string? Language,
    string? ImageUrl,
    string? ImageCaption,
    string? CalloutType);