using System.Security.Claims;
using Cyberius.Api.Common.Extensions;
using Cyberius.Api.Common.Filters;
using Cyberius.Application.Features.Users.DTOs;
using Cyberius.Application.Features.Users.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Cyberius.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("api/users")
            .WithDisplayName("User Endpoints")
            .WithTags("User")
            .AddEndpointFilter<RequestLoggingFilter>();
        
        group.MapGet("/me", Me)
            .RequireAuthorization(options =>
            {
                options.RequireClaim(ClaimTypes.NameIdentifier);   
            })
            .WithSummary("Me");
        
        group.MapPut("/{id:guid}", Update)
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .WithRequestValidation<UpdateUserRequest>()
            .WithSummary("Update user");
        
        group.MapPut("/{userId:guid}/change-password", ChangePassword)
            .RequireAuthorization()
            .WithRequestValidation<ChangePasswordRequest>()
            .WithSummary("Change password");
        
        return group;
    }

    private static async Task<IResult> Me(HttpContext context, IUserService userService,
        CancellationToken cancellationToken)
    {
        var userId = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                     ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var response = await userService.Me(Guid.Parse(userId), cancellationToken);
        return response.ToHttpResponse();
    }

    private static async Task<IResult> Update(
        Guid id,
        [FromForm] UpdateUserRequest request,
        IFormFile? avatar,
        IUserService userService,
        CancellationToken cancellationToken) =>
        await userService.UpdateUserAsync(id, avatar, request, cancellationToken).ToHttpResponseAsync();

    private static async Task<IResult> ChangePassword(
        Guid userId,
        ChangePasswordRequest request,
        IUserService userService,
        CancellationToken cancellationToken) =>
        await userService.ChangePasswordAsync(userId, request, cancellationToken).ToHttpResponseAsync();
}