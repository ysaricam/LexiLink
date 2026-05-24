using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Hint.Application.Contracts;
using LexiLink.Modules.Hint.Application.PlayerHintInventories.EnsurePlayerHintInventoryExists;
using LexiLink.Modules.Players.IntegrationEvents;

namespace LexiLink.Modules.Hint.Application.PlayerHintInventories.ProcessIntegrationEvents;

internal class PlayerRegisteredIntegrationEventHandler :
    IIntegrationEventHandler<PlayerRegisteredIntegrationEvent>
{
    private readonly IHintModule _hintModule;

    internal PlayerRegisteredIntegrationEventHandler(IHintModule hintModule)
    {
        _hintModule = hintModule;
    }

    public Task Handle(PlayerRegisteredIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        _hintModule.ExecuteCommandAsync(
            new EnsurePlayerHintInventoryExistsCommand(integrationEvent.PlayerId),
            cancellationToken);
}
