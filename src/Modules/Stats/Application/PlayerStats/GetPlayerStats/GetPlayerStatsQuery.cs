using LexiLink.Modules.Stats.Application.Contracts;

namespace LexiLink.Modules.Stats.Application.PlayerStats.GetPlayerStats;

public class GetPlayerStatsQuery : QueryBase<PlayerStatsDto?>
{
    public Guid PlayerId { get; }

    public GetPlayerStatsQuery(Guid playerId)
    {
        PlayerId = playerId;
    }
}
