using Cyberius.Application.Features.Blog.Comments.Models;
using Cyberius.Application.Features.Blog.Interfaces;
using Cyberius.Application.Features.Blog.Posts.Models;
using Cyberius.Application.Features.Notifications.Interfaces;
using Cyberius.Domain.Entities;
using Cyberius.Domain.Entities.Enums;
using Cyberius.Domain.Interfaces;
using Cyberius.Domain.Shared;

namespace Cyberius.Application.Features.Blog.Comments.Services;

public sealed class CommentService(IUnitOfWork uow, INotificationService notifications) : ICommentService
{
    public async Task<Result<PagedResponse<CommentResponse>>> GetByPostAsync(
        Guid postId, int page, int pageSize, Guid? currentUserId, CancellationToken ct = default)
    {
        var post = await uow.Posts.GetByIdAsync(postId, ct);
        if (post is null)
            return Errors.Post.NotFound(postId.ToString());

        var (items, total) = await uow.Comments.GetByPostIdAsync(postId, page, pageSize, ct);

        var commentIds = items
            .SelectMany(c => c.Replies.Append(c))
            .Select(c => c.Id)
            .ToList();

        var reactionCounts = await uow.CommentReactions
            .GetCountsByCommentsAsync(commentIds, ct);

        var responses = items
            .Select(c => MapComment(c, currentUserId, reactionCounts))
            .ToList();

        var totalPages = (int)Math.Ceiling((double)total / pageSize);
        return new PagedResponse<CommentResponse>(responses, total, page, pageSize, totalPages);
    }

    public async Task<Result<CommentResponse>> CreateAsync(
        Guid authorId, CreateCommentRequest request, CancellationToken ct = default)
    {
        var post = await uow.Posts.GetByIdAsync(request.PostId, ct);
        if (post is null)
            return Errors.Post.NotFound(request.PostId.ToString());

        // Проверяем родительский комментарий если это ответ
        if (request.ParentCommentId.HasValue)
        {
            var parent = await uow.Comments.GetByIdAsync(request.ParentCommentId.Value, ct);
            if (parent is null)
                return Errors.Comment.NotFound(request.ParentCommentId.Value.ToString());

            // Нельзя отвечать на ответ — только на корневой комментарий
            if (parent.ParentCommentId.HasValue)
                return Errors.Comment.CannotReplyToReply();

            // Родительский комментарий должен принадлежать той же статье
            if (parent.PostId != request.PostId)
                return Errors.Comment.PostMismatch();
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PostId = request.PostId,
            AuthorId = authorId,
            Content = request.Content.Trim(),
            ParentCommentId = request.ParentCommentId,
            IsEdited = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await uow.Comments.AddAsync(comment, ct);
        await uow.SaveChangesAsync(ct);

        var created = await uow.Comments.GetWithAuthorAsync(comment.Id, ct);
        var actor = created!.Author;
        var actorName = $"{actor.FirstName} {actor.LastName}".Trim();
        var postObj = await uow.Posts.GetByIdAsync(request.PostId, ct);

        if (request.ParentCommentId.HasValue)
        {
            // Ответ на комментарий — уведомляем автора родительского комментария
            var parent = await uow.Comments.GetByIdAsync(request.ParentCommentId.Value, ct);
            if (parent is not null && parent.AuthorId != authorId)
            {
                _ = notifications.SendCommentReplyAsync(
                    parent.AuthorId,
                    actorName,
                    actor.AvatarObjectName,
                    postObj?.Title ?? "",
                    postObj?.Slug ?? "",
                    ct);
            }
        }
        else if (postObj is not null && postObj.AuthorId != authorId)
        {
            // Новый корневой комментарий — уведомляем автора статьи
            _ = notifications.SendCommentReplyAsync(
                postObj.AuthorId,
                actorName,
                actor.AvatarObjectName,
                postObj.Title,
                postObj.Slug,
                ct);
        }

        return MapComment(created!, null, []);
    }

    public async Task<Result<CommentResponse>> UpdateAsync(
        Guid commentId, Guid currentUserId, UpdateCommentRequest request, CancellationToken ct = default)
    {
        var comment = await uow.Comments.GetByIdAsync(commentId, ct);
        if (comment is null)
            return Errors.Comment.NotFound(commentId.ToString());

        if (comment.AuthorId != currentUserId)
            return Errors.Comment.NotAuthor();

        comment.Content = request.Content.Trim();
        comment.IsEdited = true;
        comment.UpdatedAt = DateTime.UtcNow;

        uow.Comments.Update(comment);
        await uow.SaveChangesAsync(ct);

        var updated = await uow.Comments.GetWithAuthorAsync(commentId, ct);
        var reactionCounts = await uow.CommentReactions
            .GetCountsByCommentsAsync([commentId], ct);

        return MapComment(updated!, currentUserId, reactionCounts);
    }

    public async Task<Result> DeleteAsync(
        Guid commentId, Guid currentUserId, CancellationToken ct = default)
    {
        var comment = await uow.Comments.GetByIdAsync(commentId, ct);
        if (comment is null)
            return Errors.Comment.NotFound(commentId.ToString());

        if (comment.AuthorId != currentUserId)
            return Errors.Comment.NotAuthor();

        // Soft delete — текст скрывается, но ответы остаются
        comment.IsDeleted = true;
        comment.Content = "[Комментарий удалён]";
        comment.UpdatedAt = DateTime.UtcNow;

        uow.Comments.Update(comment);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ReactAsync(
        Guid commentId, Guid userId, ReactionType type, CancellationToken ct = default)
    {
        var comment = await uow.Comments.GetByIdAsync(commentId, ct);
        if (comment is null)
            return Errors.Comment.NotFound(commentId.ToString());

        var existing = await uow.CommentReactions
            .GetByCommentAndUserAsync(commentId, userId, ct);

        if (existing is not null && existing.Type == type)
        {
            await uow.CommentReactions.RemoveByCommentAndUserAsync(commentId, userId, ct);
        }
        else
        {
            if (existing is not null)
                await uow.CommentReactions.RemoveByCommentAndUserAsync(commentId, userId, ct);

            await uow.CommentReactions.AddAsync(new CommentReaction
            {
                Id = Guid.NewGuid(),
                CommentId = commentId,
                UserId = userId,
                Type = type,
                CreatedAt = DateTime.UtcNow,
            }, ct);
        }

        await uow.SaveChangesAsync(ct);

        // Уведомляем автора комментария о реакции (только при добавлении, не при удалении)
        if (!(existing is not null && existing.Type == type))
        {
            var comment2 = await uow.Comments.GetWithAuthorAsync(commentId, ct);
            var reactor = await uow.Users.GetByIdAsync(userId, ct);
            if (comment2 is not null && reactor is not null && comment2.AuthorId != userId)
            {
                var reactorName = $"{reactor.FirstName} {reactor.LastName}".Trim();
                var post = await uow.Posts.GetByIdAsync(comment2.PostId, ct);
                _ = notifications.SendCommentReactionAsync(
                    comment2.AuthorId,
                    reactorName,
                    reactor.AvatarObjectName,
                    type.ToString(),
                    post?.Title ?? "",
                    post?.Slug ?? "",
                    ct);
            }
        }

        return Result.Success();
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static CommentResponse MapComment(
        Comment c,
        Guid? currentUserId,
        Dictionary<Guid, Dictionary<ReactionType, int>> reactionCounts)
    {
        var counts = reactionCounts.GetValueOrDefault(c.Id, []);
        var myReaction = currentUserId.HasValue
            ? c.Reactions?.FirstOrDefault(r => r.UserId == currentUserId.Value)?.Type.ToString()
            : null;

        return new CommentResponse(
            Id: c.Id,
            Content: c.IsDeleted ? "[Комментарий удалён]" : c.Content,
            IsEdited: c.IsEdited,
            IsDeleted: c.IsDeleted,
            CreatedAt: c.CreatedAt,
            UpdatedAt: c.UpdatedAt,
            Author: new CommentAuthorDto(
                c.Author.UserId,
                c.Author.UserName ?? "",
                $"{c.Author.FirstName} {c.Author.LastName}".Trim(),
                c.Author.AvatarObjectName),
            Reactions: counts.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value),
            MyReaction: myReaction,
            ReplyCount: c.Replies?.Count(r => !r.IsDeleted) ?? 0,
            Replies: c.Replies?
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.CreatedAt)
                .Select(r => MapComment(r, currentUserId, reactionCounts))
                .ToList());
    }
}