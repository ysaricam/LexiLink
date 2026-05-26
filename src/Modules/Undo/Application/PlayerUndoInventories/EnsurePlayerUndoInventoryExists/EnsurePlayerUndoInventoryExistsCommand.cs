using LexiLink.Modules.Undo.Application.Contracts;

namespace LexiLink.Modules.Undo.Application.PlayerUndoInventories.EnsurePlayerUndoInventoryExists;

public class EnsurePlayerUndoInventoryExistsCommand : CommandBase
{
    public Guid PlayerId { get; }

    public EnsurePlayerUndoInventoryExistsCommand(Guid playerId)
    {
        PlayerId = playerId;
    }
}
