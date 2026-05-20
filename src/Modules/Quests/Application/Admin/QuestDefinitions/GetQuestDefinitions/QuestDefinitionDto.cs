using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.GetQuestDefinitions;

public sealed record QuestDefinitionDto(
    Guid Id,
    QuestType QuestType,
    QuestCadence Cadence,
    int Goal,
    int RewardAmount,
    QuestType? PrerequisiteQuestType,
    bool IsActive);
