using LexiLink.Modules.Diamond.Application.Configuration.CrossModule;
using LexiLink.Modules.Diamond.Application.Contracts;
using LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.ConsumePlayerDiamond;

namespace LexiLink.API.CrossModule;

// API-host adapter for Market -> Diamond charging. The Diamond module owns the
// mutation; Market only depends on the public gateway contract.
internal class DiamondGuard : IDiamondGuard
{
    private readonly IDiamondModule _diamondModule;

    public DiamondGuard(IDiamondModule diamondModule)
    {
        _diamondModule = diamondModule;
    }

    public Task EnsureDiamondAvailableAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default)
    {
        return _diamondModule.ExecuteCommandAsync(
            new ConsumePlayerDiamondCommand(playerId, amount),
            cancellationToken);
    }
}
