using LexiLink.Modules.Quests.Application.Contracts;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.ClaimQuest;

public class ClaimQuestCommand : CommandBase
{
    public Guid PlayerQuestId { get; }
    public Guid PlayerId { get; }

    public ClaimQuestCommand(Guid playerQuestId, Guid playerId)
    {
        PlayerQuestId = playerQuestId;
        PlayerId = playerId;
    }
}
