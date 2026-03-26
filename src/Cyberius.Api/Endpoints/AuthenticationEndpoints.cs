using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Cyberius.Api.Common.Extensions;
using Cyberius.Api.Common.Filters;
using Cyberius.Application.Features.Authentication.DTOs;
using Cyberius.Application.Features.Authentication.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cyberius.Api.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("api/auth")
            .WithDisplayName("Authentication Endpoints")
            .WithTags("Authentication")
            .AddEndpointFilter<RequestLoggingFilter>();
        
        group.MapPost("login", Login)
            .WithRequestValidation<LoginRequest>()
            .WithSummary("Login");
        
        group.MapPost("register", Register)
            .WithRequestValidation<RegisterRequest>()
            .WithSummary("Registration");
        
        group.MapPost("refresh-token", RefreshToken)
            .WithRequestValidation<RefreshTokenRequest>()
            .WithSummary("Refresh Token");
        
        group.MapPost("logout/{userId:guid}", Logout)
            .RequireAuthorization(options =>
            {
                options.RequireClaim(ClaimTypes.NameIdentifier);   
            })
            .WithSummary("Logout");
        
        return group;
    }

    private static async Task<IResult> Login(IAuthenticationService authenticationService, LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authenticationService.LoginAsync(request, cancellationToken);
        return response.ToHttpResponse();
    }

    private static async Task<IResult> Register(IAuthenticationService authenticationService, RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authenticationService.RegisterAsync(request, cancellationToken);
        return response.ToHttpResponse();
    }

    private static async Task<IResult> RefreshToken(IAuthenticationService authenticationService,
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authenticationService.RefreshTokenAsync(request, cancellationToken);
        return response.ToHttpResponse();
    }

    private static async Task<IResult> Logout(
        IAuthenticationService authenticationService,
        [FromRoute]Guid userId,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var requestUserId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                            ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var response = await authenticationService.LogoutAsync(Guid.Parse(requestUserId), userId, cancellationToken);
        return response.ToHttpResponse();
    }
}