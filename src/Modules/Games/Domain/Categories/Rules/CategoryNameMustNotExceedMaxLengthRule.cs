using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Categories.Rules;

public class CategoryNameMustNotExceedMaxLengthRule : IBusinessRule
{
    public const int MaxLength = 100;

    private readonly string? _name;

    public CategoryNameMustNotExceedMaxLengthRule(string? name)
    {
        _name = name;
    }

    public bool IsBroken() => _name is not null && _name.Length > MaxLength;

    public string Message => $"Category name cannot exceed {MaxLength} characters.";
}
