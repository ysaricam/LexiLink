using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Rules;

internal sealed class NameMustNotExceedMaxLengthRule : IBusinessRule
{
    private readonly string _name;
    private readonly int _maxLength;

    internal NameMustNotExceedMaxLengthRule(string name, int maxLength)
    {
        _name = name;
        _maxLength = maxLength;
    }

    public bool IsBroken() => _name.Length > _maxLength;

    public string Message => $"Name must not exceed {_maxLength} characters.";
}
