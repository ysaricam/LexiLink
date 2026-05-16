using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Players.IntegrationEvents;

namespace LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;

internal class PlayerRegisteredIntegrationEventHandler :
    IIntegrationEventHandler<PlayerRegisteredIntegrationEvent>
{
    private readonly IStatsInbox _statsInbox;

    internal PlayerRegisteredIntegrationEventHandler(IStatsInbox statsInbox)
    {
        _statsInbox = statsInbox;
    }

    public Task Handle(PlayerRegisteredIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        _statsInbox.AddAsync(integrationEvent, cancellationToken);
}
