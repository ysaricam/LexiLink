namespace LexiLink.Modules.Quests.Application.PlayerQuests.GetActiveQuests;

public record PlayerQuestDto(
    Guid Id,
    Guid PlayerId,
    string QuestType,
    string State,
    int Progress,
    int Goal,
    int RewardAmount,
    DateTime IssuedAt,
    DateTime? CompletedAt,
    DateTime? ClaimedAt,
    DateTime? ExpiresAt);
