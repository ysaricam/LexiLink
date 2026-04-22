using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.GameLinks;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class GameStartedDomainEvent : DomainEventBase
{
    public GameId GameId { get; }
    public LinkId StartLinkId { get; }
    public LinkId TargetLinkId { get; }
    public int TargetDepth { get; }
    public int MaxSteps { get; }

    public GameStartedDomainEvent(
        GameId gameId, 
        LinkId startLinkId, 
        LinkId targetLinkId, 
        int targetDepth, 
        int maxSteps)
    {
        GameId = gameId;
        StartLinkId = startLinkId;
        TargetLinkId = targetLinkId;
        TargetDepth = targetDepth;
        MaxSteps = maxSteps;
    }
}