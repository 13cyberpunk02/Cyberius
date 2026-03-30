using Cyberius.Application.Features.Blog.Posts.Models;
using Cyberius.Domain.Entities.Enums;
using FluentValidation;

namespace Cyberius.Application.Features.Blog.Posts.Validators;

public sealed class ContentBlockValidator : AbstractValidator<CreateContentBlockRequest>
{
    // Типы блоков которые требуют текстовый Content
    private static readonly HashSet<BlockType> TextBlocks =
    [
        BlockType.Heading1, BlockType.Heading2, BlockType.Heading3,
        BlockType.Paragraph, BlockType.Quote, BlockType.Callout
    ];
 
    public ContentBlockValidator()
    {
        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order не может быть отрицательным");
 
        // Текстовые блоки — Content обязателен
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Содержимое блока обязательно")
            .MaximumLength(10_000).WithMessage("Содержимое блока не должно превышать 10 000 символов")
            .When(x => TextBlocks.Contains(x.Type));
 
        // Code блок — Content и Language обязательны
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Код не может быть пустым")
            .When(x => x.Type == BlockType.Code);
 
        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Укажите язык программирования")
            .MaximumLength(50).WithMessage("Название языка слишком длинное")
            .When(x => x.Type == BlockType.Code);
 
        // Image блок — ImageUrl обязателен
        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("URL изображения обязателен")
            .MaximumLength(1000)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Некорректный URL изображения")
            .When(x => x.Type == BlockType.Image);
 
        RuleFor(x => x.ImageCaption)
            .MaximumLength(300).WithMessage("Подпись к изображению не должна превышать 300 символов")
            .When(x => x.ImageCaption is not null);
 
        // Callout — тип callout обязателен
        RuleFor(x => x.CalloutType)
            .NotEmpty().WithMessage("Тип callout обязателен")
            .Must(t => new[] { "info", "warning", "tip", "danger" }.Contains(t))
            .WithMessage("Допустимые типы: info, warning, tip, danger")
            .When(x => x.Type == BlockType.Callout);
    }
}