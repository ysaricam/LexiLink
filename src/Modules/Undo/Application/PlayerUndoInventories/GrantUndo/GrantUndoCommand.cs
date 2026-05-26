using LexiLink.Modules.Undo.Application.Contracts;

namespace LexiLink.Modules.Undo.Application.PlayerUndoInventories.GrantUndo;

public class GrantUndoCommand : CommandBase
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public GrantUndoCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }
}
