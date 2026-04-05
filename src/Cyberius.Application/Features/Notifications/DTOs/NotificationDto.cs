namespace Cyberius.Application.Features.Notifications.DTOs;

public record NotificationDto(
    string Id,
    NotificationType Type,
    string Message,
    string? PostSlug,
    string? PostTitle,
    string ActorName,
    string? ActorAvatarUrl,
    DateTime CreatedAt
);

public enum NotificationType
{
    CommentReply,
    CommentReaction,
    PostReaction,
}