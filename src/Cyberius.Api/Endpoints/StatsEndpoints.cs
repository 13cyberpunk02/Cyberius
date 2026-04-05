using Cyberius.Api.Common.Extensions;
using Cyberius.Application.Features.Blog.Stats.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cyberius.Api.Endpoints;

public static class StatsEndpoints
{
    public static IEndpointRouteBuilder MapStatsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("api/stats")
            .WithTags("Stats");
 
        group.MapGet("author/{authorId:guid}", GetAuthorStats)
            .RequireAuthorization()
            .WithSummary("Get author statistics");
 
        return group;
    }

    private static async Task<IResult> GetAuthorStats(
        [FromRoute] Guid authorId,
        IStatsService statsService,
        CancellationToken ct)
    {
        var response = await statsService.GetAuthorStatsAsync(authorId, ct);
        return response.ToHttpResponse();
    }
}