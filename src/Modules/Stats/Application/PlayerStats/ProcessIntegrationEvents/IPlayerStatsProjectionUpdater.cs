using LexiLink.Modules.Games.IntegrationEvents;
using LexiLink.Modules.Players.IntegrationEvents;

namespace LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;

public interface IPlayerStatsProjectionUpdater
{
    Task ProjectAsync(PlayerRegisteredIntegrationEvent integrationEvent, CancellationToken cancellationToken);

    Task ProjectAsync(AuthProviderLinkedIntegrationEvent integrationEvent, CancellationToken cancellationToken);

    Task ProjectAsync(PlayerProfileUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken);

    Task ProjectAsync(GameCompletedIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
