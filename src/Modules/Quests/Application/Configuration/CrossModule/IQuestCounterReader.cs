namespace LexiLink.Modules.Quests.Application.Configuration.CrossModule;

/// <summary>
/// Counters a quest definition needs to compute progress, baselines and
/// claim eligibility. Owned by Stats (Total / Today) and Players
/// (AuthProviderLinked); read into the Quests module through this
/// sync gateway. The implementation lives in the API composition root
/// (<c>LexiLink.API/CrossModule/QuestCounterReader.cs</c>) and queries
/// stats.* and players.* directly via Dapper.
/// </summary>
public sealed record QuestCounters(
    int GamesCompletedTotal,
    int GamesCompletedToday,
    bool AuthProviderLinked);

public interface IQuestCounterReader
{
    Task<QuestCounters> ReadAsync(
        Guid playerId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);
}
