using LexiLink.Modules.Reset.Application.Contracts;

namespace LexiLink.Modules.Reset.Application.PlayerResetInventories.EnsurePlayerResetInventoryExists;

public class EnsurePlayerResetInventoryExistsCommand : CommandBase
{
    public Guid PlayerId { get; }

    public EnsurePlayerResetInventoryExistsCommand(Guid playerId)
    {
        PlayerId = playerId;
    }
}
