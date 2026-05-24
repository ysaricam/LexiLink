using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Events;

public class PlayerQuestIssuedDomainEvent : DomainEvent
{
    public PlayerQuestId PlayerQuestId { get; }
    public Guid PlayerId { get; }
    public QuestDefinitionId QuestDefinitionId { get; }

    public PlayerQuestIssuedDomainEvent(
        PlayerQuestId playerQuestId,
        Guid playerId,
        QuestDefinitionId questDefinitionId)
    {
        PlayerQuestId = playerQuestId;
        PlayerId = playerId;
        QuestDefinitionId = questDefinitionId;
    }
}
