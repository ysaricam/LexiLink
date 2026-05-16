using LexiLink.Common.Application.Events;
using LexiLink.Modules.Games.Domain.Games.Events;
using Newtonsoft.Json;

namespace LexiLink.Modules.Games.Infrastructure.Outbox.DomainEventNotifications;

public class GameCompletedDomainEventNotification : IDomainEventNotification<GameCompletedDomainEvent>
{
    [JsonIgnore]
    public GameCompletedDomainEvent DomainEvent { get; private set; } = null!;

    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public Guid GameId { get; private set; }
    public Guid PlayerId { get; private set; }
    public Guid StartLinkId { get; private set; }
    public Guid TargetLinkId { get; private set; }
    public int Score { get; private set; }

    public GameCompletedDomainEventNotification(GameCompletedDomainEvent domainEvent, Guid id)
    {
        DomainEvent = domainEvent;
        Id = id;
        OccurredOn = domainEvent.OccurredOn;
        GameId = domainEvent.GameId.Value;
        PlayerId = domainEvent.PlayerId;
        StartLinkId = domainEvent.StartLinkId.Value;
        TargetLinkId = domainEvent.TargetLinkId.Value;
        Score = domainEvent.Score.Points;
    }

    [JsonConstructor]
    private GameCompletedDomainEventNotification()
    {
    }
}
