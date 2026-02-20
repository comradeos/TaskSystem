using FluentValidation;
using TaskSystem.Application.DTO.Projects;

namespace TaskSystem.Application.Validators;

public sealed class ProjectCreateRequestValidator : AbstractValidator<ProjectCreateRequest>
{
    public ProjectCreateRequestValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(3)
            .WithMessage("Project name must be at least 3 characters.")
            .MaximumLength(200)
            .WithMessage("Project name must not exceed 200 characters.");
    }
}