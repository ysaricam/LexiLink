using LexiLink.Modules.Quests.Application.Contracts;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.GetActiveQuests;

public class GetActiveQuestsQuery : QueryBase<IReadOnlyList<PlayerQuestDto>>
{
    public Guid PlayerId { get; }

    public GetActiveQuestsQuery(Guid playerId)
    {
        PlayerId = playerId;
    }
}
