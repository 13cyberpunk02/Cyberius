using Cyberius.Application.Features.Blog.Comments.Models;
using FluentValidation;

namespace Cyberius.Application.Features.Blog.Comments.Validators;

public sealed class UpdateCommentValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Комментарий не может быть пустым")
            .MinimumLength(2).WithMessage("Комментарий слишком короткий")
            .MaximumLength(2000).WithMessage("Комментарий не должен превышать 2000 символов");
    }
}