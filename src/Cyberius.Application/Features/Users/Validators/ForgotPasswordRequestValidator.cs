using Cyberius.Application.Features.Users.DTOs;
using FluentValidation;

namespace Cyberius.Application.Features.Users.Validators;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Эл. почта не может быть пустой")
            .NotNull().WithMessage("Эл. почта не может быть пустой")
            .EmailAddress().WithMessage("Неправильно набрали эл. почту");
    }
}