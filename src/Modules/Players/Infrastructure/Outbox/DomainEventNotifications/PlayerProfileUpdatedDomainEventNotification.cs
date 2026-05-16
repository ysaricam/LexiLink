using LexiLink.Common.Application.Events;
using LexiLink.Modules.Players.Domain.Players.Events;
using Newtonsoft.Json;

namespace LexiLink.Modules.Players.Infrastructure.Outbox.DomainEventNotifications;

public class PlayerProfileUpdatedDomainEventNotification : IDomainEventNotification<PlayerProfileUpdatedDomainEvent>
{
    [JsonIgnore]
    public PlayerProfileUpdatedDomainEvent DomainEvent { get; private set; } = null!;

    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public Guid PlayerId { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string Locale { get; private set; } = null!;

    public PlayerProfileUpdatedDomainEventNotification(PlayerProfileUpdatedDomainEvent domainEvent, Guid id)
    {
        DomainEvent = domainEvent;
        Id = id;
        OccurredOn = domainEvent.OccurredOn;
        PlayerId = domainEvent.PlayerId.Value;
        AvatarUrl = domainEvent.AvatarUrl;
        Locale = domainEvent.Locale;
    }

    [JsonConstructor]
    private PlayerProfileUpdatedDomainEventNotification()
    {
    }
}
