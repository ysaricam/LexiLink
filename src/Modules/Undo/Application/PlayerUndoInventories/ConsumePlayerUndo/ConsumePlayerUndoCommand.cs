using LexiLink.Modules.Undo.Application.Contracts;

namespace LexiLink.Modules.Undo.Application.PlayerUndoInventories.ConsumePlayerUndo;

public class ConsumePlayerUndoCommand : CommandBase
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public ConsumePlayerUndoCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }
}
