using Cyberius.Api.Common.Extensions;
using Cyberius.Application.Features.Newsletter.DTOs;
using Cyberius.Application.Features.Newsletter.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cyberius.Api.Endpoints;

public static class NewsletterEndpoints
{
    public static IEndpointRouteBuilder MapNewsletterEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("api/newsletter")
            .WithTags("Newsletter");
 
        group.MapPost("subscribe", Subscribe)
            .WithRequestValidation<SubscribeRequest>()
            .WithSummary("Subscribe to newsletter");
 
        group.MapGet("unsubscribe", Unsubscribe)
            .WithSummary("Unsubscribe from newsletter");
 
        group.MapPost("send", Send)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithRequestValidation<SendNewsletterRequest>()
            .WithSummary("Send newsletter to all subscribers (Admin only)");
 
        return group;
    }
 
    private static async Task<IResult> Subscribe(
        [FromBody] SubscribeRequest request,
        INewsletterService service,
        CancellationToken ct)
    {
        var result = await service.SubscribeAsync(request.Email, ct);
        return result.ToHttpResponse();
    }
 
    private static async Task<IResult> Unsubscribe(
        [FromQuery] string token,
        INewsletterService service,
        CancellationToken ct)
    {
        var result = await service.UnsubscribeAsync(token, ct);
        return result.ToHttpResponse();
    }
 
    private static async Task<IResult> Send(
        [FromBody] SendNewsletterRequest request,
        INewsletterService service,
        CancellationToken ct)
    {
        var result = await service.SendToAllAsync(request.Subject, request.HtmlBody, ct);
        return result.ToHttpResponse();
    }
}