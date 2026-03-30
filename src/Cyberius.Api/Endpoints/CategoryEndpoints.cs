using System.Security.Claims;
using Cyberius.Api.Common.Extensions;
using Cyberius.Api.Common.Filters;
using Cyberius.Application.Features.Blog.Categories.Models;
using Cyberius.Application.Features.Blog.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Cyberius.Api.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("api/categories")
            .WithDisplayName("Category Endpoints")
            .WithTags("Categories")
            .AddEndpointFilter<RequestLoggingFilter>();
 
        // ── Public ─────────────────────────────────────────────────────────
        group.MapGet("/", GetAll)
            .WithSummary("Get all categories with post count");
 
        group.MapGet("{id:guid}", GetById)
            .WithSummary("Get category by id");
 
        // ── Admin only ─────────────────────────────────────────────────────
        group.MapPost("/", Create)
            .RequireAuthorization(options =>
            {
                options.AddRequirements(new ClaimsAuthorizationRequirement(ClaimTypes.Role, ["Admin"]));
            })
            .WithRequestValidation<CreateCategoryRequest>()
            .WithSummary("Create category");
 
        group.MapPut("{id:guid}", Update)
            .RequireAuthorization(options =>
            {
                options.AddRequirements(new ClaimsAuthorizationRequirement(ClaimTypes.Role, ["Admin"]));
            })
            .WithRequestValidation<UpdateCategoryRequest>()
            .WithSummary("Update category");
 
        group.MapDelete("{id:guid}", Delete)
            .RequireAuthorization(options =>
            {
                options.AddRequirements(new ClaimsAuthorizationRequirement(ClaimTypes.Role, ["Admin"]));
            })
            .WithSummary("Delete category (fails if has posts)");
 
        return group;
    }
 
    // ── Handlers ───────────────────────────────────────────────────────────
 
    private static async Task<IResult> GetAll(
        ICategoryService categoryService,
        CancellationToken ct)
    {
        var result = await categoryService.GetAllAsync(ct);
        return result.ToHttpResponse();
    }
 
    private static async Task<IResult> GetById(
        [FromRoute] Guid id,
        ICategoryService categoryService,
        CancellationToken ct)
    {
        var result = await categoryService.GetByIdAsync(id, ct);
        return result.ToHttpResponse();
    }
 
    private static async Task<IResult> Create(
        ICategoryService categoryService,
        CreateCategoryRequest request,
        CancellationToken ct)
    {
        var result = await categoryService.CreateAsync(request, ct);
        return result.ToHttpResponse();
    }
 
    private static async Task<IResult> Update(
        [FromRoute] Guid id,
        ICategoryService categoryService,
        UpdateCategoryRequest request,
        CancellationToken ct)
    {
        var result = await categoryService.UpdateAsync(id, request, ct);
        return result.ToHttpResponse();
    }
 
    private static async Task<IResult> Delete(
        [FromRoute] Guid id,
        ICategoryService categoryService,
        CancellationToken ct)
    {
        var result = await categoryService.DeleteAsync(id, ct);
        return result.ToHttpResponse();
    }
}