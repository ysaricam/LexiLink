using LexiLink.Common.Domain;

namespace LexiLink.Modules.Payments.Domain.Events;

public sealed class IapPurchaseStatusChangedDomainEvent : DomainEvent
{
    public IapPurchaseStatusChangedDomainEvent(
        Guid iapPurchaseId,
        IapPurchaseStatus status)
    {
        IapPurchaseId = iapPurchaseId;
        Status = status;
    }

    public Guid IapPurchaseId { get; }

    public IapPurchaseStatus Status { get; }
}
