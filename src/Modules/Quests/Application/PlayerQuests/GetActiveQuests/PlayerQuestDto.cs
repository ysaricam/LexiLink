namespace LexiLink.Modules.Quests.Application.PlayerQuests.GetActiveQuests;

/// <summary>
/// View-model for a player's quest. Progress and DisplayState are
/// computed at read time from the Stats counter — they are not
/// persisted columns on PlayerQuests. DisplayState is one of
/// "Active" / "ReadyToClaim" / "Claimed". Reward is split into
/// EnergyReward + HintReward post Sprint H; either or both can be
/// positive on a given definition.
/// </summary>
public record PlayerQuestDto(
    Guid Id,
    Guid PlayerId,
    Guid QuestDefinitionId,
    string Name,
    string Description,
    string Trigger,
    string DisplayState,
    int Progress,
    int Threshold,
    int EnergyReward,
    int HintReward,
    DateTime IssuedAt,
    DateTime? ClaimedAt,
    DateTime? ExpiresAt);
