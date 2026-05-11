using FluentValidation;
using LexiLink.Modules.Games.Domain.Categories.Rules;

namespace LexiLink.Modules.Games.Application.Categories.EditCategory;

internal class EditCategoryCommandValidator : AbstractValidator<EditCategoryCommand>
{
    public EditCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(CategoryNameMustNotExceedMaxLengthRule.MaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(CategoryDescriptionMustNotExceedMaxLengthRule.MaxLength);
    }
}
