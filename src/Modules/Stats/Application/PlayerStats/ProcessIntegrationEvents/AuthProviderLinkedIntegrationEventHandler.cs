using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Players.IntegrationEvents;

namespace LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;

internal class AuthProviderLinkedIntegrationEventHandler :
    IIntegrationEventHandler<AuthProviderLinkedIntegrationEvent>
{
    private readonly IStatsInbox _statsInbox;

    internal AuthProviderLinkedIntegrationEventHandler(IStatsInbox statsInbox)
    {
        _statsInbox = statsInbox;
    }

    public Task Handle(AuthProviderLinkedIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        _statsInbox.AddAsync(integrationEvent, cancellationToken);
}
