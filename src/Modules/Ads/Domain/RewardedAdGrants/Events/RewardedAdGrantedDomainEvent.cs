using LexiLink.Common.Domain;

namespace LexiLink.Modules.Ads.Domain.RewardedAdGrants.Events;

public class RewardedAdGrantedDomainEvent : DomainEvent
{
    public Guid RewardedAdGrantId { get; }
    public Guid PlayerId { get; }
    public int DiamondAmount { get; }
    public string TransactionId { get; }
    public DateTime GrantedOn { get; }

    public RewardedAdGrantedDomainEvent(
        Guid rewardedAdGrantId,
        Guid playerId,
        int diamondAmount,
        string transactionId,
        DateTime grantedOn)
    {
        RewardedAdGrantId = rewardedAdGrantId;
        PlayerId = playerId;
        DiamondAmount = diamondAmount;
        TransactionId = transactionId;
        GrantedOn = grantedOn;
    }
}
