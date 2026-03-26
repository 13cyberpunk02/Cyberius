using System.Security.Claims;
using Cyberius.Api.Common.Extensions;
using Cyberius.Api.Common.Filters;
using Cyberius.Application.Features.Users.Interfaces;
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
}