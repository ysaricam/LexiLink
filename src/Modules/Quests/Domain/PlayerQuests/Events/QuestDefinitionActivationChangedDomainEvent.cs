using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Events;

public class QuestDefinitionActivationChangedDomainEvent : DomainEvent
{
    public Guid QuestDefinitionId { get; }
    public bool IsActive { get; }

    public QuestDefinitionActivationChangedDomainEvent(Guid questDefinitionId, bool isActive)
    {
        QuestDefinitionId = questDefinitionId;
        IsActive = isActive;
    }
}
