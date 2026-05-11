using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class GameCompletedDomainEvent : DomainEvent
{
    public GameId GameId { get; }
    public Guid PlayerId { get; }
    public LinkId StartLinkId { get; }
    public LinkId TargetLinkId { get; }
    public Score Score { get; }

    public GameCompletedDomainEvent(
        GameId gameId,
        Guid playerId,
        LinkId startLinkId,
        LinkId targetLinkId,
        Score score)
    {
        GameId = gameId;
        PlayerId = playerId;
        StartLinkId = startLinkId;
        TargetLinkId = targetLinkId;
        Score = score;
    }
}
