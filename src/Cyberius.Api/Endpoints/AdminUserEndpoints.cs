using System.Security.Claims;
using Cyberius.Api.Common.Extensions;
using Cyberius.Application.Features.Admin.DTOs;
using Cyberius.Application.Features.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cyberius.Api.Endpoints;

public static class AdminUserEndpoints
{
    public static IEndpointRouteBuilder MapAdminUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("api/admin/users")
            .WithTags("Admin");
        
        group.MapGet("/", GetAll)
            .RequireRateLimiting(RateLimitingExtensions.Public)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"))
            .WithSummary("Get all users (paged)");
 
        group.MapGet("{userId:guid}", GetById)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"))
            .WithSummary("Get user by id");
 
        group.MapPut("{userId:guid}/role", ChangeRole)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"))
            .WithSummary("Change user role");
 
        group.MapPut("{userId:guid}/toggle-block", ToggleBlock)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"))
            .WithSummary("Block / unblock user");
 
        group.MapDelete("{userId:guid}", DeleteUser)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithSummary("Delete user (Admin only)");
        
        return group;
    }
    
        private static async Task<IResult> GetAll(
        IAdminService adminService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await adminService.GetAllAsync(page, pageSize, search, ct);
        return result.ToHttpResponse();
    }
 
    private static async Task<IResult> GetById(
        [FromRoute] Guid userId,
        IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetByIdAsync(userId, ct);
        return result.ToHttpResponse();
    }
 
    private static async Task<IResult> ChangeRole(
        [FromRoute] Guid userId,
        [FromBody] ChangeRoleRequest request,
        IAdminService adminService,
        HttpContext ctx,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId(ctx);
        var result = await adminService.ChangeRoleAsync(currentUserId, userId, request.RoleName, ct);
        return result.ToHttpResponse();
    }
 
    private static async Task<IResult> ToggleBlock(
        [FromRoute] Guid userId,
        IAdminService adminService,
        HttpContext ctx,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId(ctx);
        var result = await adminService.ToggleBlockAsync(currentUserId, userId, ct);
        return result.ToHttpResponse();
    }
 
    private static async Task<IResult> DeleteUser(
        [FromRoute] Guid userId,
        IAdminService adminService,
        HttpContext ctx,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId(ctx);
        var result = await adminService.DeleteAsync(currentUserId, userId, ct);
        return result.ToHttpResponse();
    }
 
    private static Guid GetCurrentUserId(HttpContext ctx) =>
        Guid.TryParse(
            ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            out var id) ? id : Guid.Empty;
}