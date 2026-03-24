using Cyberius.Application.Features.Authentication.DTOs;
using FluentValidation;

namespace Cyberius.Application.Features.Authentication.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
         RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Имя пользователя обязательно")
            .MinimumLength(3).WithMessage("Имя пользователя должно содержать не менее 3 символов")
            .MaximumLength(50).WithMessage("Имя пользователя не должно превышать 50 символов")
            .Matches("^[a-zA-Z0-9_]*$").WithMessage("Имя пользователя может содержать только буквы, цифры и символ подчёркивания");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Электронная почта обязательна")
            .NotNull().WithMessage("Электронная почта обязательна")
            .EmailAddress().WithMessage("Неверный формат электронной почты");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(6).WithMessage("Пароль должен содержать минимум 6 символов")
            .MaximumLength(100).WithMessage("Пароль слишком длинный")
            .Matches(@"[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
            .Matches(@"[a-z]").WithMessage("Пароль должен содержать хотя бы одну строчную букву")
            .Matches(@"[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру")
            .Matches(@"[!@#$%^&*(),.?"":{}|<>]").WithMessage("Пароль должен содержать хотя бы один специальный символ");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Пароль подтверждения обязателен")
            .Equal(x => x.Password).WithMessage("Пароли не совпадают");
        
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Имя обязательно")
            .MinimumLength(3).WithMessage("Имя должно содержать не менее 3 букв")
            .MaximumLength(50).WithMessage("Имя не должно превышать 50 букв");
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Фамилия обязательна")
            .MinimumLength(3).WithMessage("Фамилия должна содержать не менее 3 букв")
            .MaximumLength(50).WithMessage("Фамилия не должна превышать 50 букв");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Укажите дату рождения")
            .NotNull().WithMessage("Укажите дату рождения")
            .Must(BeValidDate).WithMessage("Дата рождения указано неверно.");
    }
    
    private bool BeValidDate(DateTime date) => !date.Equals(default(DateTime));
}