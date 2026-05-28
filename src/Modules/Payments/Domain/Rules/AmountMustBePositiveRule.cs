using LexiLink.Common.Domain;

namespace LexiLink.Modules.Payments.Domain.Rules;

internal sealed class AmountMustBePositiveRule : IBusinessRule
{
    private readonly int _amount;
    private readonly string _fieldName;

    internal AmountMustBePositiveRule(int amount, string fieldName)
    {
        _amount = amount;
        _fieldName = fieldName;
    }

    public bool IsBroken() => _amount <= 0;

    public string Message => $"{_fieldName} must be greater than 0.";
}
