namespace LexiLink.Modules.Energy.Domain.PlayerEnergies;

public interface IEnergyConfigurationService
{
    int MaximumAmount { get; }
    int RechargeIntervalSeconds { get; }
    int GameStartCost { get; }
}
