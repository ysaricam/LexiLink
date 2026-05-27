using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Events;

public sealed class PurchaseOrderCreatedDomainEvent : DomainEvent
{
    public PurchaseOrderCreatedDomainEvent(
        Guid purchaseOrderId,
        Guid playerId,
        Guid shopItemId,
        ItemType itemType,
        int quantity,
        int diamondsPaid,
        DateTime purchasedAt,
        string idempotencyKey)
    {
        PurchaseOrderId = purchaseOrderId;
        PlayerId = playerId;
        ShopItemId = shopItemId;
        ItemType = itemType;
        Quantity = quantity;
        DiamondsPaid = diamondsPaid;
        PurchasedAt = purchasedAt;
        IdempotencyKey = idempotencyKey;
    }

    public Guid PurchaseOrderId { get; }
    public Guid PlayerId { get; }
    public Guid ShopItemId { get; }
    public ItemType ItemType { get; }
    public int Quantity { get; }
    public int DiamondsPaid { get; }
    public DateTime PurchasedAt { get; }
    public string IdempotencyKey { get; }
}
