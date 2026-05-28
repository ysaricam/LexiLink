using LexiLink.Common.Domain;

namespace LexiLink.Modules.Payments.Domain.Events;

public sealed class IapPurchaseReceivedDomainEvent : DomainEvent
{
    public IapPurchaseReceivedDomainEvent(
        Guid iapPurchaseId,
        Guid playerId,
        PaymentPlatform platform,
        string storeProductId)
    {
        IapPurchaseId = iapPurchaseId;
        PlayerId = playerId;
        Platform = platform;
        StoreProductId = storeProductId;
    }

    public Guid IapPurchaseId { get; }

    public Guid PlayerId { get; }

    public PaymentPlatform Platform { get; }

    public string StoreProductId { get; }
}
