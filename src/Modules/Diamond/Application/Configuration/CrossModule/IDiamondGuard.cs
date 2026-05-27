namespace LexiLink.Modules.Diamond.Application.Configuration.CrossModule;

// Cross-module gateway for charging Diamond from runtime workflows such as
// Market checkout. The API host implementation dispatches ConsumePlayerDiamondCommand.
public interface IDiamondGuard
{
    Task EnsureDiamondAvailableAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default);
}
