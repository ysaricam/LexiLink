using LexiLink.Modules.Stats.Application.Configuration.InternalCommands;
using LexiLink.Modules.Stats.Application.Contracts;
using LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;

namespace LexiLink.Modules.Stats.Infrastructure.InternalCommands;

internal static class StatsInternalCommandTypeMap
{
    private static readonly IReadOnlyDictionary<string, Type> NameToType = new Dictionary<string, Type>
    {
        [typeof(ProcessStatsInboxCommand).FullName!] = typeof(ProcessStatsInboxCommand)
    };

    public static string GetName(IInternalCommand command) =>
        command.GetType().FullName
        ?? throw new InvalidOperationException($"Internal command type '{command.GetType()}' has no full name.");

    public static Type? GetType(string typeName) =>
        NameToType.GetValueOrDefault(typeName);
}
