using Cyberius.Application.Features.Blog.Comments.Models;
using FluentValidation;

namespace Cyberius.Application.Features.Blog.Comments.Validators;

public sealed class CreateCommentValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Статья обязательна");
 
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Комментарий не может быть пустым")
            .MinimumLength(2).WithMessage("Комментарий слишком короткий")
            .MaximumLength(2000).WithMessage("Комментарий не должен превышать 2000 символов");
 
        // ParentCommentId опционален — только для ответов
        RuleFor(x => x.ParentCommentId)
            .NotEmpty().WithMessage("Некорректный Id родительского комментария")
            .When(x => x.ParentCommentId is not null);
    }
}