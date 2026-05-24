namespace LexiLink.Modules.Quests.Domain.PlayerQuests;

/// <summary>
/// Decides what the issued PlayerQuest's progress is measured against.
/// Only meaningful for <see cref="QuestTrigger.GameCompletedTotal"/>;
/// Daily and AuthProviderLinked ignore this value because their counters
/// already start at the relevant zero.
/// </summary>
public enum ProgressBaseline
{
    /// <summary>
    /// At issuance, snapshot the player's current counter and measure
    /// progress as <c>counter - snapshot</c>. New players see "0/N"
    /// immediately even if the quest was added later in their journey.
    /// </summary>
    FromSnapshot = 1,

    /// <summary>
    /// Snapshot is fixed at zero — progress measures the absolute
    /// counter. Useful for retroactive milestones ("you've completed 50
    /// games") that should reward longtime players on first sync.
    /// </summary>
    FromExistingTotal = 2,
}
