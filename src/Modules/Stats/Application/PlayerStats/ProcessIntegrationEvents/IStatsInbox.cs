using LexiLink.Common.Application.IntegrationEvents;

namespace LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;

public interface IStatsInbox
{
    Task AddAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
