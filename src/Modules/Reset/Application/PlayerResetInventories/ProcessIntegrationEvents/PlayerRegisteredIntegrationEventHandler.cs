using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Players.IntegrationEvents;
using LexiLink.Modules.Reset.Application.Contracts;
using LexiLink.Modules.Reset.Application.PlayerResetInventories.EnsurePlayerResetInventoryExists;

namespace LexiLink.Modules.Reset.Application.PlayerResetInventories.ProcessIntegrationEvents;

internal class PlayerRegisteredIntegrationEventHandler :
    IIntegrationEventHandler<PlayerRegisteredIntegrationEvent>
{
    private readonly IResetModule _resetModule;

    internal PlayerRegisteredIntegrationEventHandler(IResetModule resetModule)
    {
        _resetModule = resetModule;
    }

    public Task Handle(PlayerRegisteredIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        _resetModule.ExecuteCommandAsync(
            new EnsurePlayerResetInventoryExistsCommand(integrationEvent.PlayerId),
            cancellationToken);
}
