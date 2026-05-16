namespace LexiLink.Modules.Quests.Domain.PlayerQuests;

public sealed record QuestDefinition(
    QuestType Type,
    QuestCadence Cadence,
    int Goal,
    int RewardAmount,
    QuestType? PrerequisiteQuestType);
