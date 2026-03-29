using Cyberius.Application.Features.Users.DTOs;
using FluentValidation;

namespace Cyberius.Application.Features.Users.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
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
    
    private bool BeValidDate(DateOnly date) => !date.Equals(default);
}