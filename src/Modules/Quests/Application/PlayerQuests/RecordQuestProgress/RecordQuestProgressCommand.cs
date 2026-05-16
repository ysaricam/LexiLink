using LexiLink.Modules.Quests.Application.Contracts;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.RecordQuestProgress;

public class RecordQuestProgressCommand : CommandBase
{
    public Guid PlayerId { get; }
    public QuestType QuestType { get; }
    public int Delta { get; }

    public RecordQuestProgressCommand(Guid playerId, QuestType questType, int delta)
    {
        PlayerId = playerId;
        QuestType = questType;
        Delta = delta;
    }
}
