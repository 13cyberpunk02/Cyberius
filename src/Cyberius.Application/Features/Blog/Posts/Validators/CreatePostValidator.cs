using Cyberius.Application.Features.Blog.Posts.Models;
using FluentValidation;

namespace Cyberius.Application.Features.Blog.Posts.Validators;

public sealed class CreatePostValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostValidator()
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
            .NotNull().WithMessage("Список тегов не может быть null")
            .Must(t => t.Count <= 10).WithMessage("Не более 10 тегов на статью");
 
        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("Тег не может быть пустой строкой")
            .MaximumLength(100).WithMessage("Тег не должен превышать 100 символов");
 
        RuleFor(x => x.Blocks)
            .NotNull().WithMessage("Список блоков не может быть null");
 
        RuleForEach(x => x.Blocks)
            .SetValidator(new ContentBlockValidator());
    }
}