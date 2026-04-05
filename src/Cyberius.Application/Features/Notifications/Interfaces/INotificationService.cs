namespace Cyberius.Application.Features.Notifications.Interfaces;

public interface INotificationService
{
    Task SendCommentReplyAsync(
        Guid targetUserId,
        string actorName,
        string? actorAvatarUrl,
        string postTitle,
        string postSlug,
        CancellationToken ct = default);
 
    Task SendCommentReactionAsync(
        Guid targetUserId,
        string actorName,
        string? actorAvatarUrl,
        string reactionType,
        string postTitle,
        string postSlug,
        CancellationToken ct = default);
 
    Task SendPostReactionAsync(
        Guid targetUserId,
        string actorName,
        string? actorAvatarUrl,
        string reactionType,
        string postTitle,
        string postSlug,
        CancellationToken ct = default);
}