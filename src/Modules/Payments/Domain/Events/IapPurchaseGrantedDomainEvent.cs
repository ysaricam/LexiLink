using LexiLink.Common.Domain;

namespace LexiLink.Modules.Payments.Domain.Events;

public sealed class IapPurchaseGrantedDomainEvent : DomainEvent
{
    public IapPurchaseGrantedDomainEvent(
        Guid iapPurchaseId,
        Guid playerId,
        PaymentPlatform platform,
        string storeProductId,
        int diamondAmount,
        DateTime grantedAt)
    {
        IapPurchaseId = iapPurchaseId;
        PlayerId = playerId;
        Platform = platform;
        StoreProductId = storeProductId;
        DiamondAmount = diamondAmount;
        GrantedAt = grantedAt;
    }

    public Guid IapPurchaseId { get; }
    public Guid PlayerId { get; }
    public PaymentPlatform Platform { get; }
    public string StoreProductId { get; }
    public int DiamondAmount { get; }
    public DateTime GrantedAt { get; }
}
