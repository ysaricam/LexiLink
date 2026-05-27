using FluentValidation;

namespace LexiLink.Modules.Market.Application.Admin.Categories.DeactivateCategory;

internal sealed class DeactivateCategoryCommandValidator : AbstractValidator<DeactivateCategoryCommand>
{
    public DeactivateCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}
