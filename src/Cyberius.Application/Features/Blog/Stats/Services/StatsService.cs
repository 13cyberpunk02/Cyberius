using Cyberius.Application.Features.Blog.Stats.DTOs;
using Cyberius.Application.Features.Blog.Stats.Interfaces;
using Cyberius.Domain.Interfaces;

namespace Cyberius.Application.Features.Blog.Stats.Services;

public class StatsService(IUnitOfWork uow) : IStatsService
{
    public async Task<Result<AuthorStatsResponse>> GetAuthorStatsAsync(Guid authorId, CancellationToken ct)
    {
        // Все опубликованные посты автора
        var (posts, _) = await uow.Posts.GetByAuthorAsync(authorId, 1, 100, ct);

        if (!posts.Any())
            return Result<AuthorStatsResponse>.Success(new AuthorStatsResponse(0, 0, 0, [], [], []));

        var postIds = posts.Select(p => p.Id).ToList();

        // Просмотры по всем постам
        var viewCounts = await uow.PostViews.GetCountsByPostsAsync(postIds, ct);
        var totalViews = viewCounts.Values.Sum();

        // Комментарии по всем постам
        var commentCounts = await uow.Comments.GetCountsByPostsAsync(postIds, ct);
        var totalComments = commentCounts.Values.Sum();

        // Реакции
        var totalReactions = posts.Sum(p => p.Reactions?.Count ?? 0);

        // Топ-5 статей по просмотрам
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

        // Просмотры за последние 30 дней — один запрос по всем постам
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = today.AddDays(-29);

        // Последовательно — DbContext не поддерживает параллельные операции
        var dailyMap = new Dictionary<DateOnly, int>();

        foreach (var postId in postIds)
        {
            var series = await uow.PostViews.GetDailyStatsAsync(postId, from, today, ct);
            foreach (var (date, count) in series)
            {
                dailyMap.TryGetValue(date, out var existing);
                dailyMap[date] = existing + count;
            }
        }

        // Заполняем пропущенные дни нулями
        var dailyStats = Enumerable.Range(0, 30)
            .Select(i => from.AddDays(i))
            .Select(date => new DailyViewDto(
                date.ToString("yyyy-MM-dd"),
                dailyMap.GetValueOrDefault(date, 0)))
            .ToList();

        // Реакции по типам
        var reactionSummary = posts
            .Where(p => p.Reactions != null)
            .SelectMany(p => p.Reactions!)
            .GroupBy(r => r.Type.ToString())
            .Select(g => new ReactionSummaryDto(g.Key, g.Count()))
            .OrderByDescending(r => r.Count)
            .ToList();

        return Result<AuthorStatsResponse>.Success(new AuthorStatsResponse(
            TotalViews: totalViews,
            TotalComments: totalComments,
            TotalPosts: posts.Count,
            TopPosts: topPosts,
            DailyViews: dailyStats,
            Reactions: reactionSummary));
    }
}