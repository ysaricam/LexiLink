using LexiLink.Common.Domain;

namespace LexiLink.Modules.Hint.Domain.PlayerHintInventories.Rules;

public class HintAmountMustBeNonNegativeRule : IBusinessRule
{
    private readonly int _amount;

    public HintAmountMustBeNonNegativeRule(int amount)
    {
        _amount = amount;
    }

    public bool IsBroken() => _amount < 0;

    public string Message => "Hint amount cannot be negative.";
}
