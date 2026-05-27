namespace LexiLink.Modules.Reset.Application.Configuration.CrossModule;

// Cross-module gateway for granting Reset inventory from runtime workflows such as
// Market checkout. The API host implementation dispatches GrantResetCommand.
public interface IResetGrant
{
    Task GrantAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default);
}
