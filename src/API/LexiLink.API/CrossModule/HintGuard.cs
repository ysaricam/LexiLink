using LexiLink.Modules.Games.Application.Configuration.CrossModule;
using LexiLink.Modules.Hint.Application.Contracts;
using LexiLink.Modules.Hint.Application.PlayerHintInventories.ConsumePlayerHint;

namespace LexiLink.API.CrossModule;

// API-host adapter for the cross-module Games → Hint gateway. Lives in the
// composition root so neither Games nor Hint needs a structural reference to
// the other. Translates EnsureHintAvailableAsync into Hint's
// ConsumePlayerHintCommand; insufficient balance propagates the underlying
// BusinessRuleValidationException to the caller (UseHintCommandHandler).
internal class HintGuard : IHintGuard
{
    private const int DefaultConsumeAmount = 1;

    private readonly IHintModule _hintModule;

    public HintGuard(IHintModule hintModule)
    {
        _hintModule = hintModule;
    }

    public Task EnsureHintAvailableAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        return _hintModule.ExecuteCommandAsync(
            new ConsumePlayerHintCommand(playerId, DefaultConsumeAmount),
            cancellationToken);
    }
}
