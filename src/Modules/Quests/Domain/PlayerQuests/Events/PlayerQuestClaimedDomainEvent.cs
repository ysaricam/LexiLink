using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Events;

public class PlayerQuestClaimedDomainEvent : DomainEvent
{
    public PlayerQuestId PlayerQuestId { get; }
    public Guid PlayerId { get; }
    public QuestDefinitionId QuestDefinitionId { get; }
    public int EnergyReward { get; }
    public int HintReward { get; }
    public int UndoReward { get; }
    public int ResetReward { get; }

    public PlayerQuestClaimedDomainEvent(
        PlayerQuestId playerQuestId,
        Guid playerId,
        QuestDefinitionId questDefinitionId,
        int energyReward,
        int hintReward,
        int undoReward,
        int resetReward)
    {
        PlayerQuestId = playerQuestId;
        PlayerId = playerId;
        QuestDefinitionId = questDefinitionId;
        EnergyReward = energyReward;
        HintReward = hintReward;
        UndoReward = undoReward;
        ResetReward = resetReward;
    }
}
