using LexiLink.Modules.Quests.Application.Contracts;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.IssueQuest;

public class IssueQuestCommand : CommandBase
{
    public Guid PlayerId { get; }
    public QuestType QuestType { get; }

    public IssueQuestCommand(Guid playerId, QuestType questType)
    {
        PlayerId = playerId;
        QuestType = questType;
    }
}
