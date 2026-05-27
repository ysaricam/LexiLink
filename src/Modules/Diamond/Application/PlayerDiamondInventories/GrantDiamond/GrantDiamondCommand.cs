using LexiLink.Modules.Diamond.Application.Contracts;

namespace LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.GrantDiamond;

public class GrantDiamondCommand : CommandBase
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public GrantDiamondCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }
}
