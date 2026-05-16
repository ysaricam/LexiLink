using LexiLink.Modules.Stats.Application.Contracts;

namespace LexiLink.Modules.Stats.Application.PlayerStats.GetLeaderboard;

public class GetLeaderboardQuery : QueryBase<IReadOnlyList<LeaderboardEntryDto>>
{
    public LeaderboardOrderBy OrderBy { get; }
    public LeaderboardPeriod Period { get; }
    public DateTime? PeriodStartDate { get; }
    public int Limit { get; }

    public GetLeaderboardQuery(
        LeaderboardOrderBy orderBy,
        int limit,
        LeaderboardPeriod period = LeaderboardPeriod.AllTime,
        DateTime? periodStartDate = null)
    {
        OrderBy = orderBy;
        Period = period;
        PeriodStartDate = periodStartDate?.Date;
        Limit = Math.Clamp(limit, 1, 100);
    }
}
