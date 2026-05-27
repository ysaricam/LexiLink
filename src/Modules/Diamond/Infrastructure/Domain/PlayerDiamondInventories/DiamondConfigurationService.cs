using LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories;
using Microsoft.Extensions.Configuration;

namespace LexiLink.Modules.Diamond.Infrastructure.Domain.PlayerDiamondInventories;

internal class DiamondConfigurationService : IDiamondConfigurationService
{
    private const int DefaultInitialBalance = 0;

    public DiamondConfigurationService(IConfiguration configuration)
    {
        InitialBalance = ReadInt(configuration, "Diamond:InitialBalance", DefaultInitialBalance);
    }

    public int InitialBalance { get; }

    private static int ReadInt(IConfiguration configuration, string key, int defaultValue)
    {
        var raw = configuration[key];
        return int.TryParse(raw, out var value) ? value : defaultValue;
    }
}
