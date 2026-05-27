using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Rules;

internal sealed class PositiveAmountRule : IBusinessRule
{
    private readonly int _amount;
    private readonly string _name;

    internal PositiveAmountRule(int amount, string name)
    {
        _amount = amount;
        _name = name;
    }

    public bool IsBroken() => _amount <= 0;

    public string Message => $"{_name} must be greater than zero.";
}
