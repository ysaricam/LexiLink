using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Players.IntegrationEvents;
using LexiLink.Modules.Undo.Application.Contracts;
using LexiLink.Modules.Undo.Application.PlayerUndoInventories.EnsurePlayerUndoInventoryExists;

namespace LexiLink.Modules.Undo.Application.PlayerUndoInventories.ProcessIntegrationEvents;

internal class PlayerRegisteredIntegrationEventHandler :
    IIntegrationEventHandler<PlayerRegisteredIntegrationEvent>
{
    private readonly IUndoModule _undoModule;

    internal PlayerRegisteredIntegrationEventHandler(IUndoModule undoModule)
    {
        _undoModule = undoModule;
    }

    public Task Handle(PlayerRegisteredIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        _undoModule.ExecuteCommandAsync(
            new EnsurePlayerUndoInventoryExistsCommand(integrationEvent.PlayerId),
            cancellationToken);
}
