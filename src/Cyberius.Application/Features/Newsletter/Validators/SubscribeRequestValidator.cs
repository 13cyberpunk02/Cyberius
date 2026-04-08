using Cyberius.Application.Features.Newsletter.DTOs;
using FluentValidation;

namespace Cyberius.Application.Features.Newsletter.Validators;

public class SubscribeRequestValidator : AbstractValidator<SubscribeRequest>
{
    public SubscribeRequestValidator()
    {
        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Эл. почта не может быть пустой")
            .NotNull().WithMessage("Эл. почта не может быть пустой")
            .EmailAddress().WithMessage("Неправильно набрали эл. почту");
    }
}