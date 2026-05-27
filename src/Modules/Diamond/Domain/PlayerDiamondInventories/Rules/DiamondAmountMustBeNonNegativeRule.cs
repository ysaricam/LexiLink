using LexiLink.Common.Domain;

namespace LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories.Rules;

public class DiamondAmountMustBeNonNegativeRule : IBusinessRule
{
    private readonly int _amount;

    public DiamondAmountMustBeNonNegativeRule(int amount)
    {
        _amount = amount;
    }

    public bool IsBroken() => _amount < 0;

    public string Message => "Diamond amount cannot be negative.";
}
