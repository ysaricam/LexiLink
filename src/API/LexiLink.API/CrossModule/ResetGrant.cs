using LexiLink.Modules.Reset.Application.Configuration.CrossModule;
using LexiLink.Modules.Reset.Application.Contracts;
using LexiLink.Modules.Reset.Application.PlayerResetInventories.GrantReset;

namespace LexiLink.API.CrossModule;

// API-host adapter for Market -> Reset grants.
internal class ResetGrant : IResetGrant
{
    private readonly IResetModule _resetModule;

    public ResetGrant(IResetModule resetModule)
    {
        _resetModule = resetModule;
    }

    public Task GrantAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default)
    {
        return _resetModule.ExecuteCommandAsync(
            new GrantResetCommand(playerId, amount),
            cancellationToken);
    }
}
