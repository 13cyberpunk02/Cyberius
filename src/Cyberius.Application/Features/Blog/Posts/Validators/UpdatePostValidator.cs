using Cyberius.Application.Features.Blog.Posts.Models;
using FluentValidation;

namespace Cyberius.Application.Features.Blog.Posts.Validators;

public sealed class UpdatePostValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Заголовок обязателен")
            .MaximumLength(300).WithMessage("Заголовок не должен превышать 300 символов");
 
        RuleFor(x => x.Excerpt)
            .MaximumLength(500).WithMessage("Описание не должно превышать 500 символов")
            .When(x => x.Excerpt is not null);
 
        RuleFor(x => x.CoverImageUrl)
            .MaximumLength(1000).WithMessage("URL обложки слишком длинный")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Некорректный URL обложки")
            .When(x => x.CoverImageUrl is not null);
 
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Категория обязательна");
 
        RuleFor(x => x.Tags)
            .NotNull()
            .Must(t => t.Count <= 10).WithMessage("Не более 10 тегов на статью");
 
        RuleForEach(x => x.Tags)
            .NotEmpty()
            .MaximumLength(100);
 
        RuleFor(x => x.Blocks)
            .NotNull();
 
        RuleForEach(x => x.Blocks)
            .SetValidator(new ContentBlockValidator());
    }
}