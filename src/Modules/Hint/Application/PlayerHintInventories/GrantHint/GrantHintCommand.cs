using LexiLink.Modules.Hint.Application.Contracts;

namespace LexiLink.Modules.Hint.Application.PlayerHintInventories.GrantHint;

public class GrantHintCommand : CommandBase
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public GrantHintCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }
}
