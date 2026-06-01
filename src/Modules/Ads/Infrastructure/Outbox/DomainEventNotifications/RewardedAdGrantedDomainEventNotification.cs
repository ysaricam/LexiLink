using LexiLink.Common.Application.Events;
using LexiLink.Modules.Ads.Domain.RewardedAdGrants.Events;
using Newtonsoft.Json;

namespace LexiLink.Modules.Ads.Infrastructure.Outbox.DomainEventNotifications;

public sealed class RewardedAdGrantedDomainEventNotification
    : IDomainEventNotification<RewardedAdGrantedDomainEvent>
{
    [JsonIgnore]
    public RewardedAdGrantedDomainEvent DomainEvent { get; private set; } = null!;

    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public Guid RewardedAdGrantId { get; private set; }
    public Guid PlayerId { get; private set; }
    public int DiamondAmount { get; private set; }
    public string TransactionId { get; private set; } = null!;
    public DateTime GrantedOn { get; private set; }

    public RewardedAdGrantedDomainEventNotification(RewardedAdGrantedDomainEvent domainEvent, Guid id)
    {
        DomainEvent = domainEvent;
        Id = id;
        OccurredOn = domainEvent.OccurredOn;
        RewardedAdGrantId = domainEvent.RewardedAdGrantId;
        PlayerId = domainEvent.PlayerId;
        DiamondAmount = domainEvent.DiamondAmount;
        TransactionId = domainEvent.TransactionId;
        GrantedOn = domainEvent.GrantedOn;
    }

    [JsonConstructor]
    private RewardedAdGrantedDomainEventNotification()
    {
    }
}
