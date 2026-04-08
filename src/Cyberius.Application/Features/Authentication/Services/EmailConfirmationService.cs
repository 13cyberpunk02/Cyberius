using System.Security.Cryptography;
using Cyberius.Application.Features.Authentication.Interfaces;
using Cyberius.Application.Features.Email.Interfaces;
using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Domain.Shared;

namespace Cyberius.Application.Features.Authentication.Services;

public class EmailConfirmationService(
    IUnitOfWork uow,
    IEmailService email) : IEmailConfirmationService
{
    private const int TokenLifetimeHours = 24;
 
    public async Task<Result> SendConfirmationAsync(
        Guid userId, string frontendBaseUrl, CancellationToken ct = default)
    {
        var user = await uow.Users.GetByIdAsync(userId, ct);
        if (user is null)
            return Errors.NotFound(nameof(User), userId);
 
        if (user.IsEmailConfirmed)
            return Errors.BadRequest("Email уже подтверждён");
 
        // Удаляем старые токены
        await uow.EmailTokens.RemoveAllByUserAsync(userId, ct);
 
        var token = GenerateToken();
        await uow.EmailTokens.AddAsync(new EmailToken
        {
            UserId    = userId,
            Token     = token,
            ExpiresAt = DateTime.UtcNow.AddHours(TokenLifetimeHours),
        }, ct);
 
        await uow.SaveChangesAsync(ct);
 
        var confirmLink = $"{frontendBaseUrl}/confirm-email?token={token}";
        var fullName    = $"{user.FirstName} {user.LastName}".Trim();
        await email.SendEmailConfirmationAsync(user.Email, fullName, confirmLink, ct);
 
        return Result.Success();
    }
 
    public async Task<Result> ConfirmEmailAsync(string token, CancellationToken ct = default)
    {
        var confirmToken = await uow.EmailTokens.GetByTokenAsync(token, ct);
 
        if (confirmToken is null || confirmToken.IsUsed)
            return Errors.BadRequest("Ссылка недействительна или уже использована");
 
        if (confirmToken.ExpiresAt < DateTime.UtcNow)
            return Errors.BadRequest("Ссылка истекла. Запросите новую");
 
        confirmToken.IsUsed              = true;
        confirmToken.User.IsEmailConfirmed = true;
 
        uow.Users.Update(confirmToken.User);
        await uow.SaveChangesAsync(ct);
 
        return Result.Success();
    }
 
    private static string GenerateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
}