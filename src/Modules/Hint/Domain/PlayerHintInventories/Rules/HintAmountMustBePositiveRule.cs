using LexiLink.Common.Domain;

namespace LexiLink.Modules.Hint.Domain.PlayerHintInventories.Rules;

public class HintAmountMustBePositiveRule : IBusinessRule
{
    private readonly int _amount;

    public HintAmountMustBePositiveRule(int amount)
    {
        _amount = amount;
    }

    public bool IsBroken() => _amount <= 0;

    public string Message => "Hint amount must be positive.";
}
