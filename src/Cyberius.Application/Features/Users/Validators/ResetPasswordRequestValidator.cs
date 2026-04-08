using Cyberius.Application.Features.Users.DTOs;
using FluentValidation;

namespace Cyberius.Application.Features.Users.Validators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Токен обязателен к заполнению")
            .NotNull().WithMessage("Токен обязателен к заполнению");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(6).WithMessage("Пароль должен содержать минимум 6 символов")
            .MaximumLength(100).WithMessage("Пароль слишком длинный")
            .Matches(@"[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
            .Matches(@"[a-z]").WithMessage("Пароль должен содержать хотя бы одну строчную букву")
            .Matches(@"[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру")
            .Matches(@"[!@#$%^&*(),.?"":{}|<>]").WithMessage("Пароль должен содержать хотя бы один специальный символ");
    }
}