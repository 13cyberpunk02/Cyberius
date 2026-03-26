using Cyberius.Application.Features.Authentication.DTOs;
using FluentValidation;

namespace Cyberius.Application.Features.Authentication.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(r => r.AccessToken)
            .NotEmpty().WithMessage("Токен доступа обязателен к заполнению")
            .NotNull().WithMessage("Токен доступа обязателен к заполнению");

        RuleFor(r => r.RefreshToken)
            .NotEmpty().WithMessage("Токен обновления обязателен к заполнению")
            .NotNull().WithMessage("Токен обновления обязателен к заполнению");
    }
}