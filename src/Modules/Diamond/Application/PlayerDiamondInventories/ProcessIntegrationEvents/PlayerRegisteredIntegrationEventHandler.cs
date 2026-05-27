using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Diamond.Application.Contracts;
using LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.EnsurePlayerDiamondInventoryExists;
using LexiLink.Modules.Players.IntegrationEvents;

namespace LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.ProcessIntegrationEvents;

internal class PlayerRegisteredIntegrationEventHandler :
    IIntegrationEventHandler<PlayerRegisteredIntegrationEvent>
{
    private readonly IDiamondModule _diamondModule;

    internal PlayerRegisteredIntegrationEventHandler(IDiamondModule diamondModule)
    {
        _diamondModule = diamondModule;
    }

    public Task Handle(PlayerRegisteredIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        _diamondModule.ExecuteCommandAsync(
            new EnsurePlayerDiamondInventoryExistsCommand(integrationEvent.PlayerId),
            cancellationToken);
}
