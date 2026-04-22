using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class GameTimedOutDomainEvent : DomainEventBase
{
    public GameId GameId { get; }

    public GameTimedOutDomainEvent(GameId gameId)
    {
        GameId = gameId;
    }
}