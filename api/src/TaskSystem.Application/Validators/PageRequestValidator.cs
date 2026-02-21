using FluentValidation;
using TaskSystem.Application.DTO.Common;

namespace TaskSystem.Application.Validators;

public sealed class PageRequestValidator : AbstractValidator<PageRequest>
{
    public PageRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);
        RuleFor(x => x.Size)
            .InclusiveBetween(1, 100);
    }
}