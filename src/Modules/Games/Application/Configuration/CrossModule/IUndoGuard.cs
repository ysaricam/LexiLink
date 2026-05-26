namespace LexiLink.Modules.Games.Application.Configuration.CrossModule;

/// <summary>
/// Cross-module gateway from Games to the Undo module. The interface
/// lives in Games.Application so Games depends only on its own
/// surface; the API host supplies the adapter.
/// </summary>
public interface IUndoGuard
{
    Task EnsureUndoAvailableAsync(Guid playerId, CancellationToken cancellationToken = default);
}
