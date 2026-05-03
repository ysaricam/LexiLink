using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class GameCreatedDomainEvent : DomainEvent
{
    public GameId GameId { get; }

    public GameCreatedDomainEvent(GameId gameId)
    {
        GameId = gameId;
    }
}
