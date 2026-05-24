using LexiLink.Modules.Quests.Application.Contracts;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.IssueQuest;

public class IssueQuestCommand : CommandBase
{
    public Guid PlayerId { get; }
    public Guid QuestDefinitionId { get; }

    public IssueQuestCommand(Guid playerId, Guid questDefinitionId)
    {
        PlayerId = playerId;
        QuestDefinitionId = questDefinitionId;
    }
}
