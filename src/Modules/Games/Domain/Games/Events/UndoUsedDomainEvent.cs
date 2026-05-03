using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class UndoUsedDomainEvent : DomainEvent
{
    public GameId GameId { get; }

    public UndoUsedDomainEvent(GameId gameId)
    {
        GameId = gameId;
    }
}
