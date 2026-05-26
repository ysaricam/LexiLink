using LexiLink.Common.Domain;

namespace LexiLink.Modules.Undo.Domain.PlayerUndoInventories.Rules;

public class UndoAmountMustBePositiveRule : IBusinessRule
{
    private readonly int _amount;

    public UndoAmountMustBePositiveRule(int amount)
    {
        _amount = amount;
    }

    public bool IsBroken() => _amount <= 0;

    public string Message => "Undo amount must be positive.";
}
