using Cyberius.Api.Common.Extensions;
using Cyberius.Api.Common.Filters;
using Cyberius.Application.Features.Authentication.DTOs;
using Cyberius.Application.Features.Authentication.Interfaces;

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
}