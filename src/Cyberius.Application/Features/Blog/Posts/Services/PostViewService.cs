using Cyberius.Application.Features.Blog.Interfaces;
using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;

namespace Cyberius.Application.Features.Blog.Posts.Services;

public sealed class PostViewService(IUnitOfWork uow) : IPostViewService
{
    // Дедупликация: авторизованный — 24ч, анонимный — 1ч
    private static readonly TimeSpan AuthWindow  = TimeSpan.FromHours(24);
    private static readonly TimeSpan AnonWindow  = TimeSpan.FromHours(1);
 
    public async Task TrackAsync(
        Guid postId, Guid? userId, string? ipAddress,
        string? userAgent, CancellationToken ct = default)
    {
        var post = await uow.Posts.GetByIdAsync(postId, ct);
        if (post is null) return;
 
        var ipHash = ipAddress is not null ? HashIp(ipAddress) : null;
        var window = userId.HasValue ? AuthWindow : AnonWindow;
 
        // Не считаем повторные просмотры в рамках временного окна
        var alreadyViewed = await uow.PostViews.HasViewedAsync(
            postId, userId, ipHash, window, ct);
 
        if (alreadyViewed) return;
 
        await uow.PostViews.AddAsync(new PostView
        {
            Id        = Guid.NewGuid(),
            PostId    = postId,
            UserId    = userId,
            IpHash    = ipHash,
            UserAgent = userAgent?[..Math.Min(500, userAgent.Length)],
            ViewedAt  = DateTime.UtcNow,
        }, ct);
 
        await uow.SaveChangesAsync(ct);
    }
 
    // SHA-256 хэш IP — не храним сырой IP для соответствия GDPR
    private static string HashIp(string ip)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(ip));
        return Convert.ToHexString(bytes).ToLower();
    }
}