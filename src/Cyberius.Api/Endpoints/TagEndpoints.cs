using System.Security.Claims;
using Cyberius.Api.Common.Extensions;
using Cyberius.Api.Common.Filters;
using Cyberius.Application.Features.Blog.Interfaces;
using Cyberius.Application.Features.Blog.Tags.Models;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Cyberius.Api.Endpoints;

public static class TagEndpoints
{
    public static IEndpointRouteBuilder MapTagEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("api/tags")
            .WithDisplayName("Tag Endpoints")
            .WithTags("Tags")
            .AddEndpointFilter<RequestLoggingFilter>();

        // ── Public ─────────────────────────────────────────────────────────
        group.MapGet("popular", GetPopular)
            .WithSummary("Get popular tags");

        group.MapGet("{id:guid}", GetById)
            .WithSummary("Get tag by id");

        // ── Admin only ─────────────────────────────────────────────────────
        group.MapPost("/", Create)
            .RequireAuthorization(options =>
            {
                options.AddRequirements(new ClaimsAuthorizationRequirement(ClaimTypes.Role, ["Admin"]));
            })
            .WithRequestValidation<CreateTagRequest>()
            .WithSummary("Create tag");

        group.MapDelete("{id:guid}", Delete)
            .RequireAuthorization("AdminPolicy").RequireAuthorization(options =>
            {
                options.AddRequirements(new ClaimsAuthorizationRequirement(ClaimTypes.Role, ["Admin"]));
            })
            .WithSummary("Delete tag");

        return group;
    }

    // ── Handlers ───────────────────────────────────────────────────────────

    private static async Task<IResult> GetPopular(
        ITagService tagService,
        [FromQuery] int count = 20,
        CancellationToken ct = default)
    {
        var result = await tagService.GetPopularAsync(count, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> GetById(
        [FromRoute] Guid id,
        ITagService tagService,
        CancellationToken ct)
    {
        var result = await tagService.GetByIdAsync(id, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> Create(
        ITagService tagService,
        CreateTagRequest request,
        CancellationToken ct)
    {
        var result = await tagService.CreateAsync(request, ct);
        return result.ToHttpResponse();
    }

    private static async Task<IResult> Delete(
        [FromRoute] Guid id,
        ITagService tagService,
        CancellationToken ct)
    {
        var result = await tagService.DeleteAsync(id, ct);
        return result.ToHttpResponse();
    }
}