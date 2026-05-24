using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.GetQuestDefinitions;

public sealed record QuestDefinitionDto(
    Guid Id,
    string Name,
    string Description,
    QuestTrigger Trigger,
    int Threshold,
    int Reward,
    Guid? PrerequisiteQuestDefinitionId,
    ProgressBaseline ProgressBaseline,
    bool IsActive);
