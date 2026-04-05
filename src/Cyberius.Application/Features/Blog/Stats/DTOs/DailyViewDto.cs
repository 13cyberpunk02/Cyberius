namespace Cyberius.Application.Features.Blog.Stats.DTOs;

public record DailyViewDto(
    string Date,
    int    Count
);