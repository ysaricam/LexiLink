using LexiLink.Common.Application.Events;
using LexiLink.Modules.Payments.Domain.Events;
using Newtonsoft.Json;

namespace LexiLink.Modules.Payments.Infrastructure.Outbox.DomainEventNotifications;

public sealed class IapPurchaseStatusChangedDomainEventNotification
    : IDomainEventNotification<IapPurchaseStatusChangedDomainEvent>
{
    [JsonIgnore]
    public IapPurchaseStatusChangedDomainEvent DomainEvent { get; private set; } = null!;

    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public Guid IapPurchaseId { get; private set; }
    public string Status { get; private set; } = null!;

    public IapPurchaseStatusChangedDomainEventNotification(
        IapPurchaseStatusChangedDomainEvent domainEvent,
        Guid id)
    {
        DomainEvent = domainEvent;
        Id = id;
        OccurredOn = domainEvent.OccurredOn;
        IapPurchaseId = domainEvent.IapPurchaseId;
        Status = domainEvent.Status.ToString();
    }

    [JsonConstructor]
    private IapPurchaseStatusChangedDomainEventNotification()
    {
    }
}
