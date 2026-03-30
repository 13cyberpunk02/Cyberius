using Cyberius.Application.Features.Blog.Categories.Models;
using FluentValidation;

namespace Cyberius.Application.Features.Blog.Categories.Validators;

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название категории обязательно")
            .MaximumLength(100).WithMessage("Название не должно превышать 100 символов");
 
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug обязателен")
            .MaximumLength(120).WithMessage("Slug не должен превышать 120 символов")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug должен содержать только строчные буквы, цифры и дефис");
 
        RuleFor(x => x.Color)
            .Matches(@"^#[0-9a-fA-F]{6}$").WithMessage("Цвет должен быть в формате #RRGGBB")
            .When(x => x.Color is not null);
 
        RuleFor(x => x.IconUrl)
            .MaximumLength(1000)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Некорректный URL иконки")
            .When(x => x.IconUrl is not null);
    }
}