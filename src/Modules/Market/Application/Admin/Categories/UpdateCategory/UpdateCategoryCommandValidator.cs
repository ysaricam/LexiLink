using FluentValidation;

namespace LexiLink.Modules.Market.Application.Admin.Categories.UpdateCategory;

internal sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Icon).MaximumLength(64);
        RuleFor(x => x.VisibilityStartsAt.HasValue).Equal(x => x.VisibilityEndsAt.HasValue)
            .WithMessage("Visibility window start and end must be provided together.");
    }
}
