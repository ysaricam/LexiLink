namespace LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;

public interface IStatsInboxProcessor
{
    Task ProcessAsync(CancellationToken cancellationToken = default);
}
