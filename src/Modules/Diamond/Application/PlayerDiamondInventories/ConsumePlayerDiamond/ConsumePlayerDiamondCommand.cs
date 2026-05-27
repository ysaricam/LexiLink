using LexiLink.Modules.Diamond.Application.Contracts;

namespace LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.ConsumePlayerDiamond;

public class ConsumePlayerDiamondCommand : CommandBase
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public ConsumePlayerDiamondCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }
}
