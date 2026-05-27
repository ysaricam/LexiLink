using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Rules;

internal sealed class NameMustNotBeEmptyRule : IBusinessRule
{
    private readonly string _name;

    internal NameMustNotBeEmptyRule(string name)
    {
        _name = name;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_name);

    public string Message => "Name must not be empty.";
}
