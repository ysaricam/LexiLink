using LexiLink.Common.Application.IntegrationEvents;

namespace LexiLink.Modules.Quests.IntegrationEvents;

public sealed record QuestClaimedIntegrationEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid PlayerId,
    Guid PlayerQuestId,
    Guid QuestDefinitionId,
    int EnergyReward,
    int HintReward,
    int UndoReward,
    int ResetReward,
    int DiamondReward) : IIntegrationEvent;
