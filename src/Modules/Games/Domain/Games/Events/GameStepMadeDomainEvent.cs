using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.GameLinks;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class GameStepMadeDomainEvent : DomainEventBase
{
    public GameId GameId { get; }
    public LinkId CurrentLinkId { get; }
    public LinkId NextLinkId { get; }
    public int StepNumber { get; }
    public bool IsCorrectStep { get; }

    public GameStepMadeDomainEvent(
        GameId gameId, 
        LinkId currentLinkId, 
        LinkId nextLinkId, 
        int stepNumber, 
        bool isCorrectStep)
    {
        GameId = gameId;
        CurrentLinkId = currentLinkId;
        NextLinkId = nextLinkId;
        StepNumber = stepNumber;
        IsCorrectStep = isCorrectStep;
    }
}