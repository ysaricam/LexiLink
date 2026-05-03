using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class LastStepWarningIssuedDomainEvent : DomainEvent
{
    public GameId GameId { get; }

    public LastStepWarningIssuedDomainEvent(GameId gameId)
    {
        GameId = gameId;
    }
}
