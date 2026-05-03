using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class GameFailedDomainEvent : DomainEvent
{
    public GameId GameId { get; }

    public GameFailedDomainEvent(GameId gameId)
    {
        GameId = gameId;
    }
}
