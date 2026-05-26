using LexiLink.Modules.Reset.Domain.PlayerResetInventories;
using Microsoft.Extensions.Configuration;

namespace LexiLink.Modules.Reset.Infrastructure.Domain.PlayerResetInventories;

internal class ResetConfigurationService : IResetConfigurationService
{
    private const int DefaultInitialBalance = 0;

    public ResetConfigurationService(IConfiguration configuration)
    {
        InitialBalance = ReadInt(configuration, "Reset:InitialBalance", DefaultInitialBalance);
    }

    public int InitialBalance { get; }

    private static int ReadInt(IConfiguration configuration, string key, int defaultValue)
    {
        var raw = configuration[key];
        return int.TryParse(raw, out var value) ? value : defaultValue;
    }
}
