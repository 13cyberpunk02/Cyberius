namespace Cyberius.Application.Features.Users.Interfaces;

public interface IPasswordResetService
{
    Task<Result> ForgotPasswordAsync(
        string email, string frontendBaseUrl, CancellationToken ct = default);
 
    Task<Result> ResetPasswordAsync(
        string token, string newPassword, CancellationToken ct = default);
}