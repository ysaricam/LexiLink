using LexiLink.Modules.Energy.Domain.PlayerEnergies;
using Microsoft.Extensions.Configuration;

namespace LexiLink.Modules.Energy.Infrastructure.Domain.PlayerEnergies;

internal class EnergyConfigurationService : IEnergyConfigurationService
{
    private const int DefaultMaximumAmount = 5;
    private const int DefaultRechargeIntervalSeconds = 900;
    private const int DefaultGameStartCost = 1;

    public EnergyConfigurationService(IConfiguration configuration)
    {
        MaximumAmount = ReadInt(configuration, "Energy:MaxAmount", DefaultMaximumAmount);
        RechargeIntervalSeconds = ReadInt(configuration, "Energy:RechargeIntervalSeconds", DefaultRechargeIntervalSeconds);
        GameStartCost = ReadInt(configuration, "Energy:GameStartCost", DefaultGameStartCost);
    }

    public int MaximumAmount { get; }
    public int RechargeIntervalSeconds { get; }
    public int GameStartCost { get; }

    private static int ReadInt(IConfiguration configuration, string key, int defaultValue)
    {
        var raw = configuration[key];
        return int.TryParse(raw, out var value) ? value : defaultValue;
    }
}
