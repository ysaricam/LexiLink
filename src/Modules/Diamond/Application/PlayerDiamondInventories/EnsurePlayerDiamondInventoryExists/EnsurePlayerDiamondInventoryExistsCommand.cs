using LexiLink.Modules.Diamond.Application.Contracts;

namespace LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.EnsurePlayerDiamondInventoryExists;

public class EnsurePlayerDiamondInventoryExistsCommand : CommandBase
{
    public Guid PlayerId { get; }

    public EnsurePlayerDiamondInventoryExistsCommand(Guid playerId)
    {
        PlayerId = playerId;
    }
}
