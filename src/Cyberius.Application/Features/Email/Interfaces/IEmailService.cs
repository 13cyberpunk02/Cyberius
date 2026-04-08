namespace Cyberius.Application.Features.Email.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetAsync(
        string toEmail,
        string toName,
        string resetLink,
        CancellationToken ct = default);
    
    Task SendEmailConfirmationAsync(
        string toEmail,
        string toName,
        string confirmLink,
        CancellationToken ct = default);
}