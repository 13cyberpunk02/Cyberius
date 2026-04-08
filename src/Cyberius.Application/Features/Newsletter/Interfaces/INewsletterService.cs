namespace Cyberius.Application.Features.Newsletter.Interfaces;

public interface INewsletterService
{
    Task<Result> SubscribeAsync(string email, CancellationToken ct = default);
    Task<Result> UnsubscribeAsync(string token, CancellationToken ct = default);
    Task<Result> SendToAllAsync(string subject, string htmlBody, CancellationToken ct = default);
}