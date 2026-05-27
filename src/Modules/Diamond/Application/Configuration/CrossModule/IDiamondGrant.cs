namespace LexiLink.Modules.Diamond.Application.Configuration.CrossModule;

// Cross-module gateway for compensating refunds and runtime Diamond grants.
// The API host implementation dispatches GrantDiamondCommand.
public interface IDiamondGrant
{
    Task GrantAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default);
}
