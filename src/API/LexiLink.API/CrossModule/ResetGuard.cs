using LexiLink.Modules.Games.Application.Configuration.CrossModule;
using LexiLink.Modules.Reset.Application.Contracts;
using LexiLink.Modules.Reset.Application.PlayerResetInventories.ConsumePlayerReset;

namespace LexiLink.API.CrossModule;

// API-host adapter for the cross-module Games -> Reset gateway. Lives
// in the composition root so neither Games nor Reset needs a structural
// reference to the other. Insufficient balance propagates the
// underlying business-rule exception to ResetCommandHandler.
internal class ResetGuard : IResetGuard
{
    private const int DefaultConsumeAmount = 1;

    private readonly IResetModule _resetModule;

    public ResetGuard(IResetModule resetModule)
    {
        _resetModule = resetModule;
    }

    public Task EnsureResetAvailableAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        return _resetModule.ExecuteCommandAsync(
            new ConsumePlayerResetCommand(playerId, DefaultConsumeAmount),
            cancellationToken);
    }
}
