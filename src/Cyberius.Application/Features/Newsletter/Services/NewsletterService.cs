using System.Security.Cryptography;
using Cyberius.Application.Features.Email.Interfaces;
using Cyberius.Application.Features.Newsletter.Interfaces;
using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Domain.Shared;

namespace Cyberius.Application.Features.Newsletter.Services;

public class NewsletterService(IUnitOfWork uow, IEmailService emailService) : INewsletterService
{
    public async Task<Result> SubscribeAsync(string emailAddress, CancellationToken ct = default)
    {
        var normalized = emailAddress.Trim().ToLower();
 
        var existing = await uow.Newsletters.GetByEmailAsync(normalized, ct);
 
        if (existing is not null)
        {
            if (existing.IsActive)
                return Errors.BadRequest("Этот email уже подписан на рассылку");
 
            // Реактивируем если был отписан
            existing.IsActive = true;
            uow.Newsletters.Update(existing);
            await uow.SaveChangesAsync(ct);
            await emailService.SendSubscriptionConfirmationAsync(normalized, existing.UnsubToken, ct);
            return Result.Success();
        }
 
        var subscriber = new NewsletterSubscriber
        {
            Email       = normalized,
            UnsubToken  = GenerateToken(),
            IsActive    = true,
        };
 
        await uow.Newsletters.AddAsync(subscriber, ct);
        await uow.SaveChangesAsync(ct);
 
        await emailService.SendSubscriptionConfirmationAsync(normalized, subscriber.UnsubToken, ct);
 
        return Result.Success();
    }

    public async Task<Result> UnsubscribeAsync(string token, CancellationToken ct = default)
    {
        var subscriber = await uow.Newsletters.GetByTokenAsync(token, ct);
 
        if (subscriber is null)
            return Errors.NotFound(nameof(NewsletterSubscriber), string.Empty);
 
        subscriber.IsActive = false;
        uow.Newsletters.Update(subscriber);
        await uow.SaveChangesAsync(ct);
 
        return Result.Success();
    }

    public async Task<Result> SendToAllAsync(string subject, string htmlBody, CancellationToken ct = default)
    {
        var subscribers = await uow.Newsletters.GetAllActiveAsync(ct);
        if (!subscribers.Any())
            return Errors.BadRequest("Нет активных подписчиков");
 
        foreach (var s in subscribers)
        {
            try
            {
                await emailService.SendNewsletterAsync(s.Email, subject, htmlBody, s.UnsubToken, ct);
            }
            catch
            {
                // Логируем ошибку но продолжаем рассылку
            }
        }

        return Result.Success();
    }
    
    private static string GenerateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
}