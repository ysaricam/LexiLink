using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Categories.Rules;

public class CategoryDescriptionMustNotExceedMaxLengthRule : IBusinessRule
{
    public const int MaxLength = 500;

    private readonly string? _description;

    public CategoryDescriptionMustNotExceedMaxLengthRule(string? description)
    {
        _description = description;
    }

    public bool IsBroken() => _description is not null && _description.Length > MaxLength;

    public string Message => $"Category description cannot exceed {MaxLength} characters.";
}
