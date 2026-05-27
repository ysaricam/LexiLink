using LexiLink.Modules.Undo.Application.Configuration.CrossModule;
using LexiLink.Modules.Undo.Application.Contracts;
using LexiLink.Modules.Undo.Application.PlayerUndoInventories.GrantUndo;

namespace LexiLink.API.CrossModule;

// API-host adapter for Market -> Undo grants.
internal class UndoGrant : IUndoGrant
{
    private readonly IUndoModule _undoModule;

    public UndoGrant(IUndoModule undoModule)
    {
        _undoModule = undoModule;
    }

    public Task GrantAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default)
    {
        return _undoModule.ExecuteCommandAsync(
            new GrantUndoCommand(playerId, amount),
            cancellationToken);
    }
}
