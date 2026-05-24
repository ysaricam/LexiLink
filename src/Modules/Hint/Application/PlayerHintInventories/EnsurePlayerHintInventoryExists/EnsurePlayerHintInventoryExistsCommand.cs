using LexiLink.Modules.Hint.Application.Contracts;

namespace LexiLink.Modules.Hint.Application.PlayerHintInventories.EnsurePlayerHintInventoryExists;

public class EnsurePlayerHintInventoryExistsCommand : CommandBase
{
    public Guid PlayerId { get; }

    public EnsurePlayerHintInventoryExistsCommand(Guid playerId)
    {
        PlayerId = playerId;
    }
}
