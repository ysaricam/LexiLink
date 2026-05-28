using LexiLink.Common.Domain;

namespace LexiLink.Modules.Payments.Domain.Events;

public sealed class PaymentProductUpdatedDomainEvent : DomainEvent
{
    public PaymentProductUpdatedDomainEvent(Guid paymentProductId)
    {
        PaymentProductId = paymentProductId;
    }

    public Guid PaymentProductId { get; }
}
