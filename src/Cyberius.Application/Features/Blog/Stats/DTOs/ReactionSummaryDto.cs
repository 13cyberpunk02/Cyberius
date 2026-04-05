namespace Cyberius.Application.Features.Blog.Stats.DTOs;

public record ReactionSummaryDto(
    string Type,
    int    Count
);