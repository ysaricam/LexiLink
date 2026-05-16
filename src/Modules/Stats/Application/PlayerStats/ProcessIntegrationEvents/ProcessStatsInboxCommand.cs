using LexiLink.Modules.Stats.Application.Configuration.InternalCommands;
using LexiLink.Modules.Stats.Application.Contracts;

namespace LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;

public sealed class ProcessStatsInboxCommand : CommandBase, IInternalCommand
{
    public ProcessStatsInboxCommand()
    {
    }
}
