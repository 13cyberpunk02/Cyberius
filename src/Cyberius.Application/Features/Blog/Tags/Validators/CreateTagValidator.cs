using Cyberius.Application.Features.Blog.Tags.Models;
using FluentValidation;

namespace Cyberius.Application.Features.Blog.Tags.Validators;

public sealed class CreateTagValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название тега обязательно")
            .MaximumLength(100).WithMessage("Название не должно превышать 100 символов");
 
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug обязателен")
            .MaximumLength(120).WithMessage("Slug не должен превышать 120 символов")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug должен содержать только строчные буквы, цифры и дефис");
    }
}