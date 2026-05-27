using LexiLink.Modules.Hint.Application.Configuration.CrossModule;
using LexiLink.Modules.Hint.Application.Contracts;
using LexiLink.Modules.Hint.Application.PlayerHintInventories.GrantHint;

namespace LexiLink.API.CrossModule;

// API-host adapter for Market -> Hint grants.
internal class HintGrant : IHintGrant
{
    private readonly IHintModule _hintModule;

    public HintGrant(IHintModule hintModule)
    {
        _hintModule = hintModule;
    }

    public Task GrantAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default)
    {
        return _hintModule.ExecuteCommandAsync(
            new GrantHintCommand(playerId, amount),
            cancellationToken);
    }
}
