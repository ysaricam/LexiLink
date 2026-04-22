using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.GameLinks;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class GameResetToStartDomainEvent : DomainEventBase
{
    public GameId GameId { get; }
    public LinkId ResetFromLinkId { get; }
    public int StepNumberAtReset { get; }

    public GameResetToStartDomainEvent(GameId gameId, LinkId resetFromLinkId, int stepNumberAtReset)
    {
        GameId = gameId;
        ResetFromLinkId = resetFromLinkId;
        StepNumberAtReset = stepNumberAtReset;
    }
}