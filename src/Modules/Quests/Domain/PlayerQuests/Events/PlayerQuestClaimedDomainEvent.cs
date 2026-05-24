using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Events;

public class PlayerQuestClaimedDomainEvent : DomainEvent
{
    public PlayerQuestId PlayerQuestId { get; }
    public Guid PlayerId { get; }
    public QuestDefinitionId QuestDefinitionId { get; }
    public int Reward { get; }

    public PlayerQuestClaimedDomainEvent(
        PlayerQuestId playerQuestId,
        Guid playerId,
        QuestDefinitionId questDefinitionId,
        int reward)
    {
        PlayerQuestId = playerQuestId;
        PlayerId = playerId;
        QuestDefinitionId = questDefinitionId;
        Reward = reward;
    }
}
