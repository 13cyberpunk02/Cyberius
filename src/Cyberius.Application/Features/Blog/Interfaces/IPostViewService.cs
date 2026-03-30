namespace Cyberius.Application.Features.Blog.Interfaces;

public interface IPostViewService
{
    Task TrackAsync(Guid postId, Guid? userId, string? ipAddress, string? userAgent, CancellationToken ct = default);
}