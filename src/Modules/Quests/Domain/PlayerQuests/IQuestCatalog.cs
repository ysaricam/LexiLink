namespace LexiLink.Modules.Quests.Domain.PlayerQuests;

public interface IQuestCatalog
{
    QuestDefinition Resolve(QuestType type);

    IReadOnlyCollection<QuestDefinition> All { get; }
}
