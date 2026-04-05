using Cyberius.Api.Hubs;
using Cyberius.Application.Features.Notifications.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Cyberius.Api.Notifications;

public sealed class SignalRNotificationService(
    IHubContext<NotificationHub> hub) : INotificationService
{
    public Task SendCommentReplyAsync(
        Guid targetUserId, string actorName, string? actorAvatarUrl,
        string postTitle, string postSlug, CancellationToken ct = default) =>
        Send(targetUserId, new NotificationPayload(
            Id:             Guid.NewGuid().ToString(),
            Type:           "CommentReply",
            Message:        $"{actorName} ответил на ваш комментарий",
            PostSlug:       postSlug,
            PostTitle:      postTitle,
            ActorName:      actorName,
            ActorAvatarUrl: actorAvatarUrl,
            CreatedAt:      DateTime.UtcNow), ct);
 
    public Task SendCommentReactionAsync(
        Guid targetUserId, string actorName, string? actorAvatarUrl,
        string reactionType, string postTitle, string postSlug, CancellationToken ct = default) =>
        Send(targetUserId, new NotificationPayload(
            Id:             Guid.NewGuid().ToString(),
            Type:           "CommentReaction",
            Message:        $"{actorName} отреагировал {Emoji(reactionType)} на ваш комментарий",
            PostSlug:       postSlug,
            PostTitle:      postTitle,
            ActorName:      actorName,
            ActorAvatarUrl: actorAvatarUrl,
            CreatedAt:      DateTime.UtcNow), ct);
 
    public Task SendPostReactionAsync(
        Guid targetUserId, string actorName, string? actorAvatarUrl,
        string reactionType, string postTitle, string postSlug, CancellationToken ct = default) =>
        Send(targetUserId, new NotificationPayload(
            Id:             Guid.NewGuid().ToString(),
            Type:           "PostReaction",
            Message:        $"{actorName} отреагировал {Emoji(reactionType)} на статью «{postTitle}»",
            PostSlug:       postSlug,
            PostTitle:      postTitle,
            ActorName:      actorName,
            ActorAvatarUrl: actorAvatarUrl,
            CreatedAt:      DateTime.UtcNow), ct);
 
    private Task Send(Guid userId, NotificationPayload payload, CancellationToken ct) =>
        hub.Clients
           .Group($"user_{userId}")
           .SendAsync("ReceiveNotification", payload, ct);
 
    private static string Emoji(string type) => type switch
    {
        "Like"     => "👍",
        "Heart"    => "❤️",
        "Fire"     => "🔥",
        "Clap"     => "👏",
        "Thinking" => "🤔",
        _          => "👍",
    };
}
 
// Payload который улетает клиенту по SignalR
public record NotificationPayload(
    string   Id,
    string   Type,
    string   Message,
    string?  PostSlug,
    string?  PostTitle,
    string   ActorName,
    string?  ActorAvatarUrl,
    DateTime CreatedAt
);