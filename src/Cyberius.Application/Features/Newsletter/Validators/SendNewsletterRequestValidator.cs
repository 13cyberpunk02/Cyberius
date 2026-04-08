using Cyberius.Application.Features.Newsletter.DTOs;
using FluentValidation;

namespace Cyberius.Application.Features.Newsletter.Validators;

public class SendNewsletterRequestValidator : AbstractValidator<SendNewsletterRequest>
{
    public SendNewsletterRequestValidator()
    {
        RuleFor(x => x.HtmlBody)
            .NotNull().WithMessage("HTML body is required")
            .NotEmpty().WithMessage("HTML body is required");

        RuleFor(x => x.Subject)
            .NotNull().WithMessage("Subject is required")
            .NotEmpty().WithMessage("Subject is required");
    }
}