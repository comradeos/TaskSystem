using FluentValidation;
using TaskSystem.Application.DTO.Tasks;

namespace TaskSystem.Application.Validators;

public sealed class TaskCreateRequestValidator : AbstractValidator<TaskCreateRequest>
{
    public TaskCreateRequestValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0);
        RuleFor(x => x.Title)
            .MinimumLength(3)
            .MaximumLength(300);
        RuleFor(x => x.Description)
            .MinimumLength(1);
        RuleFor(x => x.Status)
            .IsInEnum();
        RuleFor(x => x.Priority)
            .IsInEnum();
    }
}