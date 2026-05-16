using LexiLink.Common.Domain;

namespace LexiLink.Modules.Energy.Domain.PlayerEnergies.Rules;

public class EnergyMustBeSufficientToConsumeRule : IBusinessRule
{
    private readonly int _currentAmount;
    private readonly int _requestedAmount;

    public EnergyMustBeSufficientToConsumeRule(int currentAmount, int requestedAmount)
    {
        _currentAmount = currentAmount;
        _requestedAmount = requestedAmount;
    }

    public bool IsBroken() => _currentAmount < _requestedAmount;

    public string Message =>
        $"Energy is insufficient: {_currentAmount} available, {_requestedAmount} required.";
}
