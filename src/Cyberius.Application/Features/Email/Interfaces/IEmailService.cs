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
    
    Task SendSubscriptionConfirmationAsync(string toEmail, string unsubToken, CancellationToken ct = default);
    Task SendNewsletterAsync(string toEmail, string subject, string htmlBody, string unsubToken, CancellationToken ct = default);
}