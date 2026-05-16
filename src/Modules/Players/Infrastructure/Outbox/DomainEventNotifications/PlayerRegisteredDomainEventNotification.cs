using LexiLink.Common.Application.Events;
using LexiLink.Modules.Players.Domain.Players.Events;
using Newtonsoft.Json;

namespace LexiLink.Modules.Players.Infrastructure.Outbox.DomainEventNotifications;

public class PlayerRegisteredDomainEventNotification : IDomainEventNotification<PlayerRegisteredDomainEvent>
{
    [JsonIgnore]
    public PlayerRegisteredDomainEvent DomainEvent { get; private set; } = null!;

    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public Guid PlayerId { get; private set; }
    public string DisplayName { get; private set; } = null!;
    public int Discriminator { get; private set; }
    public string Locale { get; private set; } = null!;
    public bool IsGuest { get; private set; }

    public PlayerRegisteredDomainEventNotification(PlayerRegisteredDomainEvent domainEvent, Guid id)
    {
        DomainEvent = domainEvent;
        Id = id;
        OccurredOn = domainEvent.OccurredOn;
        PlayerId = domainEvent.PlayerId.Value;
        DisplayName = domainEvent.DisplayName;
        Discriminator = domainEvent.Discriminator.Value;
        Locale = domainEvent.Locale;
        IsGuest = domainEvent.IsGuest;
    }

    [JsonConstructor]
    private PlayerRegisteredDomainEventNotification()
    {
    }
}
