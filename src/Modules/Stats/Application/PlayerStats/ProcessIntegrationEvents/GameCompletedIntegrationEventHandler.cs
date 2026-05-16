using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Games.IntegrationEvents;

namespace LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;

internal class GameCompletedIntegrationEventHandler :
    IIntegrationEventHandler<GameCompletedIntegrationEvent>
{
    private readonly IStatsInbox _statsInbox;

    internal GameCompletedIntegrationEventHandler(IStatsInbox statsInbox)
    {
        _statsInbox = statsInbox;
    }

    public Task Handle(GameCompletedIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        _statsInbox.AddAsync(integrationEvent, cancellationToken);
}
