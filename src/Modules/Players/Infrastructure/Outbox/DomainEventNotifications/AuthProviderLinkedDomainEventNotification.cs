using LexiLink.Common.Application.Events;
using LexiLink.Modules.Players.Domain.Players.Events;
using Newtonsoft.Json;

namespace LexiLink.Modules.Players.Infrastructure.Outbox.DomainEventNotifications;

public class AuthProviderLinkedDomainEventNotification : IDomainEventNotification<AuthProviderLinkedDomainEvent>
{
    [JsonIgnore]
    public AuthProviderLinkedDomainEvent DomainEvent { get; private set; } = null!;

    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public Guid PlayerId { get; private set; }
    public string Provider { get; private set; } = null!;
    public string ExternalId { get; private set; } = null!;

    public AuthProviderLinkedDomainEventNotification(AuthProviderLinkedDomainEvent domainEvent, Guid id)
    {
        DomainEvent = domainEvent;
        Id = id;
        OccurredOn = domainEvent.OccurredOn;
        PlayerId = domainEvent.PlayerId.Value;
        Provider = domainEvent.Provider.ToString();
        ExternalId = domainEvent.ExternalId;
    }

    [JsonConstructor]
    private AuthProviderLinkedDomainEventNotification()
    {
    }
}
