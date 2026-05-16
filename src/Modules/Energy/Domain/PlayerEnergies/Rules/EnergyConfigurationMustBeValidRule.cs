using LexiLink.Common.Domain;

namespace LexiLink.Modules.Energy.Domain.PlayerEnergies.Rules;

public class EnergyConfigurationMustBeValidRule : IBusinessRule
{
    private readonly int _maximumAmount;
    private readonly int _rechargeIntervalSeconds;

    public EnergyConfigurationMustBeValidRule(int maximumAmount, int rechargeIntervalSeconds)
    {
        _maximumAmount = maximumAmount;
        _rechargeIntervalSeconds = rechargeIntervalSeconds;
    }

    public bool IsBroken() => _maximumAmount <= 0 || _rechargeIntervalSeconds <= 0;

    public string Message =>
        "Energy configuration must have a positive maximum amount and recharge interval.";
}
