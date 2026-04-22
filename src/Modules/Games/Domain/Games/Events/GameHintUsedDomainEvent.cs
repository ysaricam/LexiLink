using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.GameLinks;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class GameHintUsedDomainEvent : DomainEventBase
{
    public GameId GameId { get; }
    public HintStatus HintStatus { get; }
    public int StepNumber { get; }
    public LinkId CurrentLinkId { get; }

    public GameHintUsedDomainEvent(GameId gameId, HintStatus hintStatus, int stepNumber, LinkId currentLinkId)
    {
        GameId = gameId;
        HintStatus = hintStatus;
        StepNumber = stepNumber;
        CurrentLinkId = currentLinkId;
    }
}