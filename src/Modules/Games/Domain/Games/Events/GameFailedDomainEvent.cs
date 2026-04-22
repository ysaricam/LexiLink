using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class GameFailedDomainEvent : DomainEventBase
{
    public GameId GameId { get; }
    public string Reason { get; }

    public GameFailedDomainEvent(GameId gameId, string reason)
    {
        GameId = gameId;
        Reason = reason;
    }
}