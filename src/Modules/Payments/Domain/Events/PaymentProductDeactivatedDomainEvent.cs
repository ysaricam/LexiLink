using LexiLink.Common.Domain;

namespace LexiLink.Modules.Payments.Domain.Events;

public sealed class PaymentProductDeactivatedDomainEvent : DomainEvent
{
    public PaymentProductDeactivatedDomainEvent(Guid paymentProductId)
    {
        PaymentProductId = paymentProductId;
    }

    public Guid PaymentProductId { get; }
}
