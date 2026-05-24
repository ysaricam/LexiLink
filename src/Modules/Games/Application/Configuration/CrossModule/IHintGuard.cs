namespace LexiLink.Modules.Games.Application.Configuration.CrossModule;

/// <summary>
/// Cross-module gateway from Games to the Hint module. The interface
/// lives in Games.Application so Games depends only on its own
/// surface; the implementation is composed in the API host
/// (<c>LexiLink.API.CrossModule.HintGuard</c>) and translates the
/// request into a <c>ConsumePlayerHintCommand</c> dispatched through
/// <c>IHintModule</c>.
///
/// Invoked by <c>UseHintCommandHandler</c> when the in-game free
/// hint allowance is exhausted; the player's persistent
/// PlayerHintInventory is debited by 1. Insufficient balance
/// propagates as <see cref="LexiLink.Common.Domain.BusinessRuleValidationException"/>
/// (HintBalanceMustBeSufficientRule), the same dual-write residual
/// risk pattern as IEnergyGuard.
/// </summary>
public interface IHintGuard
{
    Task EnsureHintAvailableAsync(Guid playerId, CancellationToken cancellationToken = default);
}
