using LexiLink.Common.Domain;

namespace LexiLink.Modules.Energy.Domain.PlayerEnergies.Rules;

public class BonusAmountMustBePositiveRule : IBusinessRule
{
    private readonly int _amount;

    public BonusAmountMustBePositiveRule(int amount)
    {
        _amount = amount;
    }

    public bool IsBroken() => _amount <= 0;

    public string Message => "Bonus energy amount must be positive.";
}
