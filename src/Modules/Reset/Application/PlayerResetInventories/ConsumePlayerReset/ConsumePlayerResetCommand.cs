using LexiLink.Modules.Reset.Application.Contracts;

namespace LexiLink.Modules.Reset.Application.PlayerResetInventories.ConsumePlayerReset;

public class ConsumePlayerResetCommand : CommandBase
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public ConsumePlayerResetCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }
}
