using LexiLink.Modules.Diamond.Application.Contracts;

namespace LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.GetPlayerDiamond;

public class GetPlayerDiamondQuery : QueryBase<PlayerDiamondSnapshotDto>
{
    public Guid PlayerId { get; }

    public GetPlayerDiamondQuery(Guid playerId)
    {
        PlayerId = playerId;
    }
}
