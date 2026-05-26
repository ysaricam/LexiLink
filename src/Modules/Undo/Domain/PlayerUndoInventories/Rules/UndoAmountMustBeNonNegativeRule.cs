using LexiLink.Common.Domain;

namespace LexiLink.Modules.Undo.Domain.PlayerUndoInventories.Rules;

public class UndoAmountMustBeNonNegativeRule : IBusinessRule
{
    private readonly int _amount;

    public UndoAmountMustBeNonNegativeRule(int amount)
    {
        _amount = amount;
    }

    public bool IsBroken() => _amount < 0;

    public string Message => "Undo amount cannot be negative.";
}
