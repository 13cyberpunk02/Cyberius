using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Cyberius.Infrastructure.Data.Repositories;

public class PostViewRepository(AppDbContext db)
    : GenericRepository<PostView>(db), IPostViewRepository
{
    private readonly AppDbContext _db = db;

    public async Task<int> GetCountByPostAsync(
        Guid postId, CancellationToken ct = default) =>
        await _db.PostViews
            .CountAsync(v => v.PostId == postId, ct);
 
    public async Task<bool> HasViewedAsync(
        Guid postId,
        Guid? userId,
        string? ipHash,
        TimeSpan window,
        CancellationToken ct = default)
    {
        var since = DateTime.UtcNow - window;
 
        return await _db.PostViews.AnyAsync(v =>
            v.PostId == postId
            && v.ViewedAt >= since
            && (
                // Авторизованный — дедупликация по userId
                (userId != null && v.UserId == userId)
                // Анонимный — дедупликация по хэшу IP
                || (userId == null && ipHash != null && v.IpHash == ipHash)
            ), ct);
    }
 
    public async Task<IReadOnlyList<(DateOnly Date, int Count)>> GetDailyStatsAsync(
        Guid postId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var fromDt = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toDt   = to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
 
        var raw = await _db.PostViews
            .Where(v => v.PostId == postId
                        && v.ViewedAt >= fromDt
                        && v.ViewedAt <= toDt)
            .GroupBy(v => v.ViewedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync(ct);
 
        return raw
            .Select(x => (DateOnly.FromDateTime(x.Date), x.Count))
            .ToList();
    }
}