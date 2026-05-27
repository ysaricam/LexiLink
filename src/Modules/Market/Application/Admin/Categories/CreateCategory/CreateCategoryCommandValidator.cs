using FluentValidation;

namespace LexiLink.Modules.Market.Application.Admin.Categories.CreateCategory;

internal sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Icon).MaximumLength(64);
        RuleFor(x => x.VisibilityStartsAt.HasValue).Equal(x => x.VisibilityEndsAt.HasValue)
            .WithMessage("Visibility window start and end must be provided together.");
    }
}
