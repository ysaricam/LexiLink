using LexiLink.Common.Domain;

namespace LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories.Rules;

public class DiamondAmountMustBePositiveRule : IBusinessRule
{
    private readonly int _amount;

    public DiamondAmountMustBePositiveRule(int amount)
    {
        _amount = amount;
    }

    public bool IsBroken() => _amount <= 0;

    public string Message => "Diamond amount must be positive.";
}
