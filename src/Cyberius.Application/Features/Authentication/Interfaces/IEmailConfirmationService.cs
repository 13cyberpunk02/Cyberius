namespace Cyberius.Application.Features.Authentication.Interfaces;

public interface IEmailConfirmationService
{
    /// <summary>Отправить письмо с подтверждением (при регистрации или повторно)</summary>
    Task<Result> SendConfirmationAsync(
        Guid userId, string frontendBaseUrl, CancellationToken ct = default);
 
    /// <summary>Подтвердить email по токену из письма</summary>
    Task<Result> ConfirmEmailAsync(string token, CancellationToken ct = default);
}