using LexiLink.Modules.Hint.Domain.PlayerHintInventories;
using Microsoft.Extensions.Configuration;

namespace LexiLink.Modules.Hint.Infrastructure.Domain.PlayerHintInventories;

internal class HintConfigurationService : IHintConfigurationService
{
    private const int DefaultInitialBalance = 0;

    public HintConfigurationService(IConfiguration configuration)
    {
        InitialBalance = ReadInt(configuration, "Hint:InitialBalance", DefaultInitialBalance);
    }

    public int InitialBalance { get; }

    private static int ReadInt(IConfiguration configuration, string key, int defaultValue)
    {
        var raw = configuration[key];
        return int.TryParse(raw, out var value) ? value : defaultValue;
    }
}
