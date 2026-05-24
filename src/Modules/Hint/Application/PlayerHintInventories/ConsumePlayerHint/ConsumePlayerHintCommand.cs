using LexiLink.Modules.Hint.Application.Contracts;

namespace LexiLink.Modules.Hint.Application.PlayerHintInventories.ConsumePlayerHint;

public class ConsumePlayerHintCommand : CommandBase
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public ConsumePlayerHintCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }
}
