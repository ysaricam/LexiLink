using LexiLink.Modules.Games.Application.Configuration.CrossModule;
using LexiLink.Modules.Undo.Application.Contracts;
using LexiLink.Modules.Undo.Application.PlayerUndoInventories.ConsumePlayerUndo;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;

namespace LexiLink.API.CrossModule;

// API-host adapter for the cross-module Games -> Undo gateway. Lives
// in the composition root so neither Games nor Undo needs a structural
// reference to the other. Insufficient balance propagates the
// underlying business-rule exception to UndoCommandHandler.
internal class UndoGuard : IUndoGuard
{
    private const int DefaultConsumeAmount = 1;

    private readonly IUndoModule _undoModule;
    private readonly IUndoConfigurationService _undoConfiguration;

    public UndoGuard(
        IUndoModule undoModule,
        IUndoConfigurationService undoConfiguration)
    {
        _undoModule = undoModule;
        _undoConfiguration = undoConfiguration;
    }

    public Task EnsureUndoAvailableAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        if (_undoConfiguration.UnlimitedGameplayUndoEnabled)
        {
            return Task.CompletedTask;
        }

        return _undoModule.ExecuteCommandAsync(
            new ConsumePlayerUndoCommand(playerId, DefaultConsumeAmount),
            cancellationToken);
    }
}
