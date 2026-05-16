using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Players.IntegrationEvents;

namespace LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;

internal class PlayerProfileUpdatedIntegrationEventHandler :
    IIntegrationEventHandler<PlayerProfileUpdatedIntegrationEvent>
{
    private readonly IStatsInbox _statsInbox;

    internal PlayerProfileUpdatedIntegrationEventHandler(IStatsInbox statsInbox)
    {
        _statsInbox = statsInbox;
    }

    public Task Handle(PlayerProfileUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        _statsInbox.AddAsync(integrationEvent, cancellationToken);
}
