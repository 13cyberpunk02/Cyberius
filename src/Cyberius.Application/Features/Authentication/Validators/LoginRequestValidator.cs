using Cyberius.Application.Features.Authentication.DTOs;
using FluentValidation;

namespace Cyberius.Application.Features.Authentication.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Эл. почта не может быть пустой")
            .NotNull().WithMessage("Эл. почта не может быть пустой")
            .EmailAddress().WithMessage("Неправильно набрали эл. почту");
        
        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(6).WithMessage("Пароль должен содержать минимум 6 символов")
            .MaximumLength(100).WithMessage("Пароль слишком длинный")
            .Matches(@"[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
            .Matches(@"[a-z]").WithMessage("Пароль должен содержать хотя бы одну строчную букву")
            .Matches(@"[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру")
            .Matches(@"[!@#$%^&*(),.?"":{}|<>]").WithMessage("Пароль должен содержать хотя бы один специальный символ");
    }
}