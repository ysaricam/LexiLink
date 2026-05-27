using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Rules;

internal sealed class NonNegativeAmountRule : IBusinessRule
{
    private readonly int _amount;
    private readonly string _name;

    internal NonNegativeAmountRule(int amount, string name)
    {
        _amount = amount;
        _name = name;
    }

    public bool IsBroken() => _amount < 0;

    public string Message => $"{_name} must be greater than or equal to zero.";
}
