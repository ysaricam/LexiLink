using LexiLink.Common.Application.Events;
using LexiLink.Modules.Market.Domain.Events;
using Newtonsoft.Json;

namespace LexiLink.Modules.Market.Infrastructure.Outbox.DomainEventNotifications;

public sealed class PurchaseCompletedDomainEventNotification
    : IDomainEventNotification<PurchaseOrderCreatedDomainEvent>
{
    [JsonIgnore]
    public PurchaseOrderCreatedDomainEvent DomainEvent { get; private set; } = null!;

    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public Guid PlayerId { get; private set; }
    public Guid PurchaseOrderId { get; private set; }
    public Guid ShopItemId { get; private set; }
    public string ItemType { get; private set; } = null!;
    public int Quantity { get; private set; }
    public int DiamondsPaid { get; private set; }
    public DateTime PurchasedAt { get; private set; }
    public string IdempotencyKey { get; private set; } = null!;

    public PurchaseCompletedDomainEventNotification(PurchaseOrderCreatedDomainEvent domainEvent, Guid id)
    {
        DomainEvent = domainEvent;
        Id = id;
        OccurredOn = domainEvent.OccurredOn;
        PlayerId = domainEvent.PlayerId;
        PurchaseOrderId = domainEvent.PurchaseOrderId;
        ShopItemId = domainEvent.ShopItemId;
        ItemType = domainEvent.ItemType.ToString();
        Quantity = domainEvent.Quantity;
        DiamondsPaid = domainEvent.DiamondsPaid;
        PurchasedAt = domainEvent.PurchasedAt;
        IdempotencyKey = domainEvent.IdempotencyKey;
    }

    [JsonConstructor]
    private PurchaseCompletedDomainEventNotification()
    {
    }
}
