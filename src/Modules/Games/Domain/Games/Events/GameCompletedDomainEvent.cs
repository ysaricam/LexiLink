using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class GameCompletedDomainEvent : DomainEvent
{
    public GameId GameId { get; }
    public Score Score { get; }

    public GameCompletedDomainEvent(GameId gameId, Score score)
    {
        GameId = gameId;
        Score = score;
    }
}
