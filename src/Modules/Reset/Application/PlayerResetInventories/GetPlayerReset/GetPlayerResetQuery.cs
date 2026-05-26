using LexiLink.Modules.Reset.Application.Contracts;

namespace LexiLink.Modules.Reset.Application.PlayerResetInventories.GetPlayerReset;

public class GetPlayerResetQuery : QueryBase<PlayerResetSnapshotDto>
{
    public Guid PlayerId { get; }

    public GetPlayerResetQuery(Guid playerId)
    {
        PlayerId = playerId;
    }
}
