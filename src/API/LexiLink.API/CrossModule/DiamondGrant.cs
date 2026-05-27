using LexiLink.Modules.Diamond.Application.Configuration.CrossModule;
using LexiLink.Modules.Diamond.Application.Contracts;
using LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.GrantDiamond;

namespace LexiLink.API.CrossModule;

// API-host adapter for compensating Diamond refunds from Market checkout.
internal class DiamondGrant : IDiamondGrant
{
    private readonly IDiamondModule _diamondModule;

    public DiamondGrant(IDiamondModule diamondModule)
    {
        _diamondModule = diamondModule;
    }

    public Task GrantAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default)
    {
        return _diamondModule.ExecuteCommandAsync(
            new GrantDiamondCommand(playerId, amount),
            cancellationToken);
    }
}
