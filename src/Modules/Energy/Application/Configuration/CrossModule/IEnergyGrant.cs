namespace LexiLink.Modules.Energy.Application.Configuration.CrossModule;

// Cross-module gateway for granting Energy from runtime workflows such as
// Market checkout. The API host implementation dispatches GrantEnergyCommand.
public interface IEnergyGrant
{
    Task EnsureCanAcceptAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default);

    Task GrantAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default);
}
