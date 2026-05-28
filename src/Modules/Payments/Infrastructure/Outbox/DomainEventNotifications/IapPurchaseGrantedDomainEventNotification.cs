using LexiLink.Common.Application.Events;
using LexiLink.Modules.Payments.Domain.Events;
using Newtonsoft.Json;

namespace LexiLink.Modules.Payments.Infrastructure.Outbox.DomainEventNotifications;

public sealed class IapPurchaseGrantedDomainEventNotification
    : IDomainEventNotification<IapPurchaseGrantedDomainEvent>
{
    [JsonIgnore]
    public IapPurchaseGrantedDomainEvent DomainEvent { get; private set; } = null!;

    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public Guid IapPurchaseId { get; private set; }
    public Guid PlayerId { get; private set; }
    public string Platform { get; private set; } = null!;
    public string StoreProductId { get; private set; } = null!;
    public int DiamondAmount { get; private set; }
    public DateTime GrantedAt { get; private set; }

    public IapPurchaseGrantedDomainEventNotification(IapPurchaseGrantedDomainEvent domainEvent, Guid id)
    {
        DomainEvent = domainEvent;
        Id = id;
        OccurredOn = domainEvent.OccurredOn;
        IapPurchaseId = domainEvent.IapPurchaseId;
        PlayerId = domainEvent.PlayerId;
        Platform = domainEvent.Platform.ToString();
        StoreProductId = domainEvent.StoreProductId;
        DiamondAmount = domainEvent.DiamondAmount;
        GrantedAt = domainEvent.GrantedAt;
    }

    [JsonConstructor]
    private IapPurchaseGrantedDomainEventNotification()
    {
    }
}
