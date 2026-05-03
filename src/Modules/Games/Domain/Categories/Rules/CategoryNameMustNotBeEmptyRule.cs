using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Categories.Rules;

public class CategoryNameMustNotBeEmptyRule : IBusinessRule
{
    private readonly string? _name;

    public CategoryNameMustNotBeEmptyRule(string? name)
    {
        _name = name;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_name);

    public string Message => "Category name cannot be empty.";
}
