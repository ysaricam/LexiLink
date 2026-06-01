using FluentValidation;
using LexiLink.Modules.Games.Domain.Categories.Rules;

namespace LexiLink.Modules.Games.Application.Categories.CreateCategory;

internal class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(CategoryNameMustNotExceedMaxLengthRule.MaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(CategoryDescriptionMustNotExceedMaxLengthRule.MaxLength);

        RuleFor(x => x.Language)
            .NotEmpty()
            .Matches("^[a-z]{2}-[A-Z]{2}$");
    }
}
