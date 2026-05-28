using LexiLink.Common.Domain;

namespace LexiLink.Modules.Payments.Domain.Events;

public sealed class PaymentProductCreatedDomainEvent : DomainEvent
{
    public PaymentProductCreatedDomainEvent(Guid paymentProductId, string storeProductId)
    {
        PaymentProductId = paymentProductId;
        StoreProductId = storeProductId;
    }

    public Guid PaymentProductId { get; }

    public string StoreProductId { get; }
}
