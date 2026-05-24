namespace LexiLink.Modules.Quests.Domain.PlayerQuests;

/// <summary>
/// Persisted state of a <see cref="PlayerQuest"/>. ReadyToClaim is
/// derived at read time from <c>counter - baseline &gt;= threshold</c>;
/// Expired Daily rows are deleted on the next sync rather than persisted.
/// </summary>
public enum QuestState
{
    Active = 1,
    Claimed = 3,
}
