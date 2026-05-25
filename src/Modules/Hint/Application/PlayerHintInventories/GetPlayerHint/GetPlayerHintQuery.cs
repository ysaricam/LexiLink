using LexiLink.Modules.Hint.Application.Contracts;

namespace LexiLink.Modules.Hint.Application.PlayerHintInventories.GetPlayerHint;

public class GetPlayerHintQuery : QueryBase<PlayerHintSnapshotDto>
{
    public Guid PlayerId { get; }

    public GetPlayerHintQuery(Guid playerId)
    {
        PlayerId = playerId;
    }
}
