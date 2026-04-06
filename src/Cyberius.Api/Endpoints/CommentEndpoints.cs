using System.Security.Claims;
using Cyberius.Api.Common.Extensions;
using Cyberius.Api.Common.Filters;
using Cyberius.Application.Features.Blog.Comments.Models;
using Cyberius.Application.Features.Blog.Interfaces;
using Cyberius.Domain.Entities.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Cyberius.Api.Endpoints;

public static class CommentEndpoints
{
    public static IEndpointRouteBuilder MapCommentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("api/comments")
            .WithDisplayName("Comment Endpoints")
            .WithTags("Comments")
            .AddEndpointFilter<RequestLoggingFilter>();

        // ── Public ─────────────────────────────────────────────────────────
        group.MapGet("post/{postId:guid}", GetByPost)
            .RequireRateLimiting(RateLimitingExtensions.Public)
            .WithSummary("Get comments for a post (paged)");

        // ── Authorized ─────────────────────────────────────────────────────
        group.MapPost("/", Create)
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingExtensions.Comments)
            .WithRequestValidation<CreateCommentRequest>()
            .WithSummary("Create comment or reply");

        group.MapPut("{id:guid}", Update)
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingExtensions.Mutations)
            .WithRequestValidation<UpdateCommentRequest>()
            .WithSummary("Update comment");

        group.MapDelete("{id:guid}", Delete)
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingExtensions.Mutations)
            .WithSummary("Soft-delete comment");

        group.MapPost("{id:guid}/react/{type}", React)
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingExtensions.Reactions)
            .WithSummary("React to comment (toggle)");

        return group;
    }

    // ── Handlers ───────────────────────────────────────────────────────────

    private static async Task<IResult> GetByPost(
        [FromRoute] Guid postId,
        ICommentService commentService,
        HttpContext httpContext,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentUserId = GetCurrentUserId(httpContext);
        var result = await commentService.GetByPostAsync(postId, page, pageSize, currentUserId, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> Create(
        ICommentService commentService,
        CreateCommentRequest request,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var authorId = GetCurrentUserIdRequired(httpContext);
        var result = await commentService.CreateAsync(authorId, request, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> Update(
        [FromRoute] Guid id,
        ICommentService commentService,
        UpdateCommentRequest request,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserIdRequired(httpContext);
        var result = await commentService.UpdateAsync(id, currentUserId, request, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> Delete(
        [FromRoute] Guid id,
        ICommentService commentService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserIdRequired(httpContext);
        var result = await commentService.DeleteAsync(id, currentUserId, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> React(
        [FromRoute] Guid id,
        [FromRoute] string type,
        ICommentService commentService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (!Enum.TryParse<ReactionType>(type, ignoreCase: true, out var reactionType))
            return Results.BadRequest($"Неизвестный тип реакции: '{type}'");

        var userId = GetCurrentUserIdRequired(httpContext);
        var result = await commentService.ReactAsync(id, userId, reactionType, ct);
        return result.ToHttpResponse();
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Guid? GetCurrentUserId(HttpContext ctx)
    {
        var raw = ctx.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(raw, out var id) ? id : null;
    }

    private static Guid GetCurrentUserIdRequired(HttpContext ctx) =>
        GetCurrentUserId(ctx) ?? throw new UnauthorizedAccessException();
}