using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class GameAbandonedDomainEvent : DomainEvent
{
    public GameId GameId { get; }

    public GameAbandonedDomainEvent(GameId gameId)
    {
        GameId = gameId;
    }
}
