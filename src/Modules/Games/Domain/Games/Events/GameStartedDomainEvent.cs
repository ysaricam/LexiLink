using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class GameStartedDomainEvent : DomainEvent
{
    public GameId GameId { get; }

    public GameStartedDomainEvent(GameId gameId)
    {
        GameId = gameId;
    }
}
