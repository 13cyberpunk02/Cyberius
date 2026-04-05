using Cyberius.Application.Features.Blog.Stats.DTOs;
using Cyberius.Application.Features.Blog.Stats.Interfaces;
using Cyberius.Domain.Interfaces;

namespace Cyberius.Application.Features.Blog.Stats.Services;

public class StatsService(IUnitOfWork uow) : IStatsService
{
    public async Task<Result<AuthorStatsResponse>> GetAuthorStatsAsync(Guid authorId, CancellationToken ct)
    {
        var (posts, _) = await uow.Posts.GetByAuthorAsync(authorId, 1, 100, ct);
        if (!posts.Any())
            return Result<AuthorStatsResponse>.Success(new AuthorStatsResponse(0, 0, 0, [], [], []));
        
        var postIds = posts.Select(p => p.Id).ToList();
        
        var viewCounts    = await uow.PostViews.GetCountsByPostsAsync(postIds, ct);
        var totalViews    = viewCounts.Values.Sum();
        
        var commentCounts = await uow.Comments.GetCountsByPostsAsync(postIds, ct);
        var totalComments = commentCounts.Values.Sum();

        var totalReactions = posts.Sum(p => p.Reactions?.Count ?? 0);
        
        var topPosts = posts
            .Select(p => new TopPostDto(
                p.Id,
                p.Title,
                p.Slug,
                viewCounts.GetValueOrDefault(p.Id, 0),
                commentCounts.GetValueOrDefault(p.Id, 0)))
            .OrderByDescending(p => p.ViewCount)
            .Take(5)
            .ToList();
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from  = today.AddDays(-29);
        
        var dailyStatsTasks = postIds.Select(id =>
            uow.PostViews.GetDailyStatsAsync(id, from, today, ct));
        var dailyResults = await Task.WhenAll(dailyStatsTasks);
        
        var dailyMap = new Dictionary<DateOnly, int>();
        foreach (var series in dailyResults)
            foreach (var (date, count) in series)
            {
                dailyMap.TryGetValue(date, out var existing);
                dailyMap[date] = existing + count;
            }
        
        var dailyStats = Enumerable.Range(0, 30)
            .Select(i => from.AddDays(i))
            .Select(date => new DailyViewDto(
                date.ToString("yyyy-MM-dd"),
                dailyMap.GetValueOrDefault(date, 0)))
            .ToList();
        
        var reactionSummary = posts
            .Where(p => p.Reactions != null)
            .SelectMany(p => p.Reactions!)
            .GroupBy(r => r.Type.ToString())
            .Select(g => new ReactionSummaryDto(g.Key, g.Count()))
            .OrderByDescending(r => r.Count)
            .ToList();

        return Result<AuthorStatsResponse>.Success(new AuthorStatsResponse(
            TotalViews:    totalViews,
            TotalComments: totalComments,
            TotalPosts:    posts.Count,
            TopPosts:      topPosts,
            DailyViews:    dailyStats,
            Reactions:     reactionSummary));
    }
}