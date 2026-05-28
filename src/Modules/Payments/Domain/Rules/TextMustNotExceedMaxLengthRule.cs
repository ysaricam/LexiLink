using LexiLink.Common.Domain;

namespace LexiLink.Modules.Payments.Domain.Rules;

internal sealed class TextMustNotExceedMaxLengthRule : IBusinessRule
{
    private readonly string? _value;
    private readonly int _maxLength;
    private readonly string _fieldName;

    internal TextMustNotExceedMaxLengthRule(string? value, int maxLength, string fieldName)
    {
        _value = value;
        _maxLength = maxLength;
        _fieldName = fieldName;
    }

    public bool IsBroken() => _value is not null && _value.Length > _maxLength;

    public string Message => $"{_fieldName} must not exceed {_maxLength} characters.";
}
