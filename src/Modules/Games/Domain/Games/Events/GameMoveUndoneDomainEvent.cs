using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.GameLinks;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class GameMoveUndoneDomainEvent : DomainEventBase
{
    public GameId GameId { get; }
    public LinkId CurrentLinkId { get; }
    public LinkId PreviousLinkId { get; }

    public GameMoveUndoneDomainEvent(GameId gameId, LinkId currentLinkId, LinkId previousLinkId)
    {
        GameId = gameId;
        CurrentLinkId = currentLinkId;
        PreviousLinkId = previousLinkId;
    }
}