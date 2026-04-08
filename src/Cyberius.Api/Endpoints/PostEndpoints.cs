using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Cyberius.Api.Common.Extensions;
using Cyberius.Api.Common.Filters;
using Cyberius.Application.Features.Blog.Interfaces;
using Cyberius.Application.Features.Blog.Posts.Models;
using Cyberius.Domain.Entities.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Cyberius.Api.Endpoints;

public static class PostEndpoints
{
    public static IEndpointRouteBuilder MapPostEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("api/posts")
            .WithDisplayName("Post Endpoints")
            .WithTags("Posts")
            .AddEndpointFilter<RequestLoggingFilter>();

        // ── Public ─────────────────────────────────────────────────────────
        group.MapGet("/", GetPublished)
            .RequireRateLimiting(RateLimitingExtensions.Public)
            .WithPublicCache(30)
            .WithSummary("Get published posts (paged)");

        group.MapGet("{id:guid}", GetById)
            .RequireRateLimiting(RateLimitingExtensions.Public)
            .WithPublicCache(60)
            .WithSummary("Get post by id");

        group.MapGet("slug/{slug}", GetBySlug)
            .RequireRateLimiting(RateLimitingExtensions.Public)
            .WithPublicCache(60)
            .WithSummary("Get post by slug");

        group.MapGet("search", Search)
            .RequireRateLimiting(RateLimitingExtensions.Public)
            .WithShortCache()
            .WithSummary("Full-text search posts");

        group.MapGet("category/{categoryId:guid}", GetByCategory)
            .RequireRateLimiting(RateLimitingExtensions.Public)
            .WithPublicCache(60)
            .WithSummary("Get posts by category");

        group.MapGet("tag/{tagSlug}", GetByTag)
            .RequireRateLimiting(RateLimitingExtensions.Public)
            .WithPublicCache(60)
            .WithSummary("Get posts by tag");

        group.MapGet("{id:guid}/related", GetRelated)
            .RequireRateLimiting(RateLimitingExtensions.Public)
            .WithPublicCache(120)
            .WithSummary("Get related posts");

        group.MapGet("{id:guid}/neighbors", GetNeighbors)
            .RequireRateLimiting(RateLimitingExtensions.Public)
            .WithPublicCache(60)
            .WithSummary("Get prev/next posts by date");

        group.MapGet("author/{authorId:guid}", GetByAuthor)
            .RequireRateLimiting(RateLimitingExtensions.Public)
            .WithPublicCache(30)
            .WithSummary("Get posts by author (public)");

        // ── Authorized ─────────────────────────────────────────────────────
        group.MapGet("drafts", GetDrafts)
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingExtensions.Public)
            .WithSummary("Get author's drafts");

        group.MapPost("/", Create)
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingExtensions.Mutations)
            .WithRequestValidation<CreatePostRequest>()
            .WithSummary("Create post");

        group.MapPut("{id:guid}", Update)
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingExtensions.Mutations)
            .WithRequestValidation<UpdatePostRequest>()
            .WithSummary("Update post");

        group.MapPost("{id:guid}/publish", Publish)
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingExtensions.Mutations)
            .WithSummary("Publish post");

        group.MapPost("{id:guid}/unpublish", Unpublish)
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingExtensions.Mutations)
            .WithSummary("Unpublish post (back to draft)");

        group.MapDelete("{id:guid}", Delete)
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingExtensions.Mutations)
            .WithSummary("Delete post");

        group.MapPost("{id:guid}/react/{type}", React)
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingExtensions.Reactions)
            .WithSummary("React to post (toggle)");

        group.MapPost("{id:guid}/view", TrackView)
            .RequireRateLimiting(RateLimitingExtensions.Views)
            .WithSummary("Track post view");

        return group;
    }

    // ── Handlers ───────────────────────────────────────────────────────────

    private static async Task<IResult> GetPublished(
        IPostService postService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await postService.GetPublishedAsync(page, pageSize, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> GetById(
        [FromRoute] Guid id,
        IPostService postService,
        IPostViewService viewService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId(httpContext);
        var result = await postService.GetByIdAsync(id, currentUserId, ct);

        if (result.IsSuccess)
        {
            await viewService.TrackAsync(
                id, currentUserId,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                httpContext.Request.Headers.UserAgent,
                ct);
        }

        return result.ToHttpResponse();
    }

    private static async Task<IResult> GetBySlug(
        [FromRoute] string slug,
        IPostService postService,
        IPostViewService viewService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId(httpContext);
        var result = await postService.GetBySlugAsync(slug, currentUserId, ct);

        if (result.IsSuccess)
        {
            await viewService.TrackAsync(
                result.Value.Id, currentUserId,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                httpContext.Request.Headers.UserAgent,
                ct);
        }

        return result.ToHttpResponse();
    }

    private static async Task<IResult> Search(
        IPostService postService,
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await postService.SearchAsync(q, page, pageSize, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> GetByCategory(
        [FromRoute] Guid categoryId,
        IPostService postService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await postService.GetByCategoryAsync(categoryId, page, pageSize, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> GetByTag(
        [FromRoute] string tagSlug,
        IPostService postService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await postService.GetByTagAsync(tagSlug, page, pageSize, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> GetByAuthor(
        [FromRoute] Guid authorId,
        IPostService postService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 9,
        CancellationToken ct = default)
    {
        var result = await postService.GetByAuthorAsync(authorId, page, pageSize, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> GetDrafts(
        IPostService postService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var authorId = GetCurrentUserIdRequired(httpContext);
        var result = await postService.GetDraftsByAuthorAsync(authorId, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> Create(
        IPostService postService,
        CreatePostRequest request,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var authorId = GetCurrentUserIdRequired(httpContext);
        var result = await postService.CreateAsync(authorId, request, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> Update(
        [FromRoute] Guid id,
        IPostService postService,
        UpdatePostRequest request,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserIdRequired(httpContext);
        var result = await postService.UpdateAsync(id, currentUserId, request, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> Publish(
        [FromRoute] Guid id,
        IPostService postService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserIdRequired(httpContext);
        var result = await postService.PublishAsync(id, currentUserId, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> Unpublish(
        [FromRoute] Guid id,
        IPostService postService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserIdRequired(httpContext);
        var result = await postService.UnpublishAsync(id, currentUserId, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> Delete(
        [FromRoute] Guid id,
        IPostService postService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserIdRequired(httpContext);
        var isPrivileged = httpContext.User.IsInRole("Admin") || httpContext.User.IsInRole("Manager");
        var result = await postService.DeleteAsync(id, currentUserId, isPrivileged, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> GetRelated(
        [FromRoute] Guid id,
        IPostService postService,
        [FromQuery] int count = 3,
        CancellationToken ct = default)
    {
        var result = await postService.GetRelatedAsync(id, count, ct);
        return result.ToHttpResponse();
    }
    
    private static async Task<IResult> GetNeighbors(
        [FromRoute] Guid id,
        IPostService postService,
        CancellationToken ct)
    {
        var response = await postService.GetNeighboursAsync(id, ct);
        return response.ToHttpResponse();
    }

    private static async Task<IResult> React(
        [FromRoute] Guid id,
        [FromRoute] string type,
        IPostService postService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (!Enum.TryParse<ReactionType>(type, ignoreCase: true, out var reactionType))
            return Results.BadRequest($"Неизвестный тип реакции: '{type}'");

        var userId = GetCurrentUserIdRequired(httpContext);
        var result = await postService.ReactAsync(id, userId, reactionType, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> TrackView(
        [FromRoute] Guid id,
        IPostViewService viewService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId(httpContext);
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        await viewService.TrackAsync(id, userId, ipAddress, userAgent, ct);
        return Results.NoContent();
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