using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;
using Microsoft.Extensions.Configuration;

namespace LexiLink.Modules.Undo.Infrastructure.Domain.PlayerUndoInventories;

internal class UndoConfigurationService : IUndoConfigurationService
{
    private const int DefaultInitialBalance = 0;
    private const int DefaultUnlimitedGameplayBalance = 999_999;

    public UndoConfigurationService(IConfiguration configuration)
    {
        InitialBalance = ReadInt(configuration, "Undo:InitialBalance", DefaultInitialBalance);
        UnlimitedGameplayUndoEnabled = ReadBool(configuration, "Undo:UnlimitedGameplayUndo", defaultValue: false);
        UnlimitedGameplayBalance = ReadPositiveInt(
            configuration,
            "Undo:UnlimitedGameplayBalance",
            DefaultUnlimitedGameplayBalance);
    }

    public int InitialBalance { get; }
    public bool UnlimitedGameplayUndoEnabled { get; }
    public int UnlimitedGameplayBalance { get; }

    private static int ReadInt(IConfiguration configuration, string key, int defaultValue)
    {
        var raw = configuration[key];
        return int.TryParse(raw, out var value) ? value : defaultValue;
    }

    private static int ReadPositiveInt(IConfiguration configuration, string key, int defaultValue)
    {
        var value = ReadInt(configuration, key, defaultValue);
        return value > 0 ? value : defaultValue;
    }

    private static bool ReadBool(IConfiguration configuration, string key, bool defaultValue)
    {
        var raw = configuration[key];
        return bool.TryParse(raw, out var value) ? value : defaultValue;
    }
}
