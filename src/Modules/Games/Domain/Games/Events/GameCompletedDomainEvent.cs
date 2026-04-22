using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.Score;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class GameCompletedDomainEvent : DomainEventBase
{
    public GameId GameId { get; }
    public ScoreValue FinalScore { get; }
    public int TotalSteps { get; }

    public GameCompletedDomainEvent(GameId gameId, ScoreValue finalScore, int totalSteps)
    {
        GameId = gameId;
        FinalScore = finalScore;
        TotalSteps = totalSteps;
    }
}