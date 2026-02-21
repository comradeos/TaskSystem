using FluentValidation;
using TaskSystem.Application.DTO.Tasks;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Application.Validators;

public sealed class ChangeTaskStatusRequestValidator : AbstractValidator<ChangeTaskStatusRequest>
{
    public ChangeTaskStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(status => Enum.IsDefined(typeof(TaskStatus), status))
            .WithMessage("Invalid task status value.");
    }
}