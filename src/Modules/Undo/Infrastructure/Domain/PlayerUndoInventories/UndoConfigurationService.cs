using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;
using Microsoft.Extensions.Configuration;

namespace LexiLink.Modules.Undo.Infrastructure.Domain.PlayerUndoInventories;

internal class UndoConfigurationService : IUndoConfigurationService
{
    private const int DefaultInitialBalance = 0;

    public UndoConfigurationService(IConfiguration configuration)
    {
        InitialBalance = ReadInt(configuration, "Undo:InitialBalance", DefaultInitialBalance);
    }

    public int InitialBalance { get; }

    private static int ReadInt(IConfiguration configuration, string key, int defaultValue)
    {
        var raw = configuration[key];
        return int.TryParse(raw, out var value) ? value : defaultValue;
    }
}
