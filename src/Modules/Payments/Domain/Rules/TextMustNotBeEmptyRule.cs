using LexiLink.Common.Domain;

namespace LexiLink.Modules.Payments.Domain.Rules;

internal sealed class TextMustNotBeEmptyRule : IBusinessRule
{
    private readonly string? _value;
    private readonly string _fieldName;

    internal TextMustNotBeEmptyRule(string? value, string fieldName)
    {
        _value = value;
        _fieldName = fieldName;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_value);

    public string Message => $"{_fieldName} must not be empty.";
}
