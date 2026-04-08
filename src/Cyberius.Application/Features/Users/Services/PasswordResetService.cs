using System.Security.Cryptography;
using Cyberius.Application.Features.Email.Interfaces;
using Cyberius.Application.Features.Users.Interfaces;
using Cyberius.Domain.Entities;
using Cyberius.Domain.Interfaces;
using Cyberius.Domain.Shared;

namespace Cyberius.Application.Features.Users.Services;

public class PasswordResetService(IUnitOfWork uow, IEmailService emailService) : IPasswordResetService
{
    private const int TokenLifetimeMinutes = 15;

    public async Task<Result> ForgotPasswordAsync(
        string emailAddress, string frontendBaseUrl, CancellationToken ct = default)
    {
        var user = await uow.Users.GetByEmailAsync(emailAddress, ct);

        if (user is null || !user.IsActive || user.IsDeleted)
            return Result.Success();
        
        await uow.EmailTokens.RemoveAllByUserAsync(user.UserId, ct);
        
        var token = GenerateToken();
        await uow.EmailTokens.AddAsync(new EmailToken
        {
            UserId = user.UserId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(TokenLifetimeMinutes),
        }, ct);

        await uow.SaveChangesAsync(ct);
        
        var resetLink = $"{frontendBaseUrl}/reset-password?token={token}";
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        await emailService.SendPasswordResetAsync(user.Email, fullName, resetLink, ct);

        return Result.Success();
    }
    
    public async Task<Result> ResetPasswordAsync(
        string token, string newPassword, CancellationToken ct = default)
    {
        var resetToken = await uow.EmailTokens.GetByTokenAsync(token, ct);

        if (resetToken is null || resetToken.IsUsed)
            return Errors.BadRequest("Ссылка недействительна или уже использована");

        if (resetToken.ExpiresAt < DateTime.UtcNow)
            return Errors.BadRequest("Ссылка истекла. Запросите новую");

        var user = resetToken.User;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        resetToken.IsUsed = true;

        await uow.RefreshTokens.RemoveByUserAsync(user.UserId, ct);

        uow.Users.Update(user);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static string GenerateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}