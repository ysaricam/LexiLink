using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Events;

public class PlayerQuestAdminResetDomainEvent : DomainEvent
{
    public PlayerQuestId PlayerQuestId { get; }
    public Guid PlayerId { get; }
    public QuestType QuestType { get; }

    public PlayerQuestAdminResetDomainEvent(
        PlayerQuestId playerQuestId,
        Guid playerId,
        QuestType questType)
    {
        PlayerQuestId = playerQuestId;
        PlayerId = playerId;
        QuestType = questType;
    }
}
