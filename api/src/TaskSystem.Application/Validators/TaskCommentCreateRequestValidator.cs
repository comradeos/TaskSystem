using FluentValidation;
using TaskSystem.Application.DTO.Tasks;

namespace TaskSystem.Application.Validators;

public sealed class TaskCommentCreateRequestValidator
    : AbstractValidator<CommentCreateRequest>
{
    public TaskCommentCreateRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(1000);
    }
}