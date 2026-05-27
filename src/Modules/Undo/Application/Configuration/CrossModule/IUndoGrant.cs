namespace LexiLink.Modules.Undo.Application.Configuration.CrossModule;

// Cross-module gateway for granting Undo inventory from runtime workflows such as
// Market checkout. The API host implementation dispatches GrantUndoCommand.
public interface IUndoGrant
{
    Task GrantAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default);
}
