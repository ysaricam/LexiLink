namespace LexiLink.Modules.Hint.Application.Configuration.CrossModule;

// Cross-module gateway for granting Hint inventory from runtime workflows such as
// Market checkout. The API host implementation dispatches GrantHintCommand.
public interface IHintGrant
{
    Task GrantAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default);
}
