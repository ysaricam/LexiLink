using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Games.Puzzles;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class HintUsedDomainEvent : DomainEvent
{
    public GameId GameId { get; }
    public HintResult HintResult { get; }

    public HintUsedDomainEvent(GameId gameId, HintResult hintResult)
    {
        GameId = gameId;
        HintResult = hintResult;
    }
}
