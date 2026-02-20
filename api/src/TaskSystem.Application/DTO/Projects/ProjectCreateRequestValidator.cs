using FluentValidation;

namespace TaskSystem.Application.DTO.Projects;

public sealed class ProjectCreateRequestValidator  : AbstractValidator<ProjectCreateRequest>
{
    public ProjectCreateRequestValidator()
    {
        RuleFor(x => x.Name)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Project name is required.")
            .MinimumLength(3)
            .WithMessage("Project name must be at least 3 characters.")
            .MaximumLength(200)
            .WithMessage("Project name must not exceed 200 characters.");
    }
}