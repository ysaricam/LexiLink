using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Quests.IntegrationEvents;
using LexiLink.Modules.Undo.Application.Contracts;
using LexiLink.Modules.Undo.Application.PlayerUndoInventories.EnsurePlayerUndoInventoryExists;
using LexiLink.Modules.Undo.Application.PlayerUndoInventories.GrantUndo;

namespace LexiLink.Modules.Undo.Application.PlayerUndoInventories.ProcessIntegrationEvents;

internal class QuestClaimedIntegrationEventHandler :
    IIntegrationEventHandler<QuestClaimedIntegrationEvent>
{
    private readonly IUndoModule _undoModule;

    internal QuestClaimedIntegrationEventHandler(IUndoModule undoModule)
    {
        _undoModule = undoModule;
    }

    public async Task Handle(QuestClaimedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        if (integrationEvent.UndoReward <= 0)
        {
            return;
        }

        await _undoModule.ExecuteCommandAsync(
            new EnsurePlayerUndoInventoryExistsCommand(integrationEvent.PlayerId),
            cancellationToken);

        await _undoModule.ExecuteCommandAsync(
            new GrantUndoCommand(integrationEvent.PlayerId, integrationEvent.UndoReward),
            cancellationToken);
    }
}
