using LexiLink.Modules.Reset.Application.Contracts;

namespace LexiLink.Modules.Reset.Application.PlayerResetInventories.GrantReset;

public class GrantResetCommand : CommandBase
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public GrantResetCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }
}
