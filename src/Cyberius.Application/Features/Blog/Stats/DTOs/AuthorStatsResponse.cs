namespace Cyberius.Application.Features.Blog.Stats.DTOs;

public record AuthorStatsResponse(
    int TotalViews,
    int TotalComments,
    int TotalPosts,
    List<TopPostDto> TopPosts,
    List<DailyViewDto> DailyViews,
    List<ReactionSummaryDto> Reactions
);