using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class StepMadeDomainEvent : DomainEvent
{
    public GameId GameId { get; }
    public LinkId LinkId { get; }

    public StepMadeDomainEvent(GameId gameId, LinkId linkId)
    {
        GameId = gameId;
        LinkId = linkId;
    }
}
