using LexiLink.Common.Domain;

namespace LexiLink.Modules.Undo.Domain.PlayerUndoInventories.Rules;

public class UndoBalanceMustBeSufficientRule : IBusinessRule
{
    private readonly int _currentBalance;
    private readonly int _requestedAmount;

    public UndoBalanceMustBeSufficientRule(int currentBalance, int requestedAmount)
    {
        _currentBalance = currentBalance;
        _requestedAmount = requestedAmount;
    }

    public bool IsBroken() => _currentBalance < _requestedAmount;

    public string Message =>
        $"Undo balance is insufficient: {_currentBalance} available, {_requestedAmount} required.";
}
