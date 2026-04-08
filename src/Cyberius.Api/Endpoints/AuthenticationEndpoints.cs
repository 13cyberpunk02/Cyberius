using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Cyberius.Api.Common.Extensions;
using Cyberius.Api.Common.Filters;
using Cyberius.Application.Features.Authentication.DTOs;
using Cyberius.Application.Features.Authentication.Interfaces;
using Cyberius.Application.Features.Users.DTOs;
using Cyberius.Application.Features.Users.Interfaces;
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
        
        group.MapPost("delete-user/{userId:guid}", DisableUser)
            .WithSummary("Disable User");
        
        group.MapGet("{userId:guid}", GetPublicProfile)
            .WithSummary("Get public user profile");
        
        group.MapPost("forgot-password", ForgotPassword)
            .WithRequestValidation<ForgotPasswordRequest>()
            .WithSummary("Send password reset email");
 
        group.MapPost("reset-password", ResetPassword)
            .WithRequestValidation<ResetPasswordRequest>()
            .WithSummary("Reset password using token");
        
        group.MapGet("confirm-email", ConfirmEmail)
            .WithSummary("Confirm email by token");
 
        group.MapPost("resend-confirmation", ResendConfirmation)
            .WithSummary("Resend confirmation email");
        
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

    private static async Task<IResult> DisableUser(Guid userId, IAuthenticationService authenticationService,
        CancellationToken cancellationToken) =>
        await authenticationService.DisableUser(userId, cancellationToken).ToHttpResponseAsync();
    
    private static async Task<IResult> GetPublicProfile(
        [FromRoute] Guid userId,
        IAuthenticationService authService,
        CancellationToken ct) => await authService.GetPublicProfileAsync(userId, ct).ToHttpResponseAsync();
    
    private static async Task<IResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        IPasswordResetService service,
        HttpContext ctx,
        CancellationToken ct)
    {
        // Берём base URL фронтенда из конфига или из Origin заголовка
        var origin = ctx.Request.Headers.Origin.FirstOrDefault()
                     ?? "http://localhost:4200";
 
        var result = await service.ForgotPasswordAsync(request.Email, origin, ct);
        return result.ToHttpResponse();
    }
 
    private static async Task<IResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        IPasswordResetService service,
        CancellationToken ct)
    {
        var result = await service.ResetPasswordAsync(request.Token, request.NewPassword, ct);
        return result.ToHttpResponse();
    }
    
    private static async Task<IResult> ConfirmEmail(
        [FromQuery] string token,
        IEmailConfirmationService service,
        CancellationToken ct)
    {
        var result = await service.ConfirmEmailAsync(token, ct);
        return result.ToHttpResponse();
    }
 
    private static async Task<IResult> ResendConfirmation(
        [FromBody] ResendConfirmationRequest request,
        IEmailConfirmationService service,
        HttpContext ctx,
        CancellationToken ct)
    {
        var origin = ctx.Request.Headers.Origin.FirstOrDefault()
                     ?? "http://localhost:4200";
 
        var result = await service.SendConfirmationAsync(request.UserId, origin, ct);
        return result.ToHttpResponse();
    }
}