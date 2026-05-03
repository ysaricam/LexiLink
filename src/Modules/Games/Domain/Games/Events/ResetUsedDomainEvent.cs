using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class ResetUsedDomainEvent : DomainEvent
{
    public GameId GameId { get; }

    public ResetUsedDomainEvent(GameId gameId)
    {
        GameId = gameId;
    }
}
