using LexiLink.Common.Domain;
using LexiLink.Modules.Ads.Domain.RewardedAdGrants.Events;
using LexiLink.Modules.Ads.Domain.RewardedAdGrants.Rules;

namespace LexiLink.Modules.Ads.Domain.RewardedAdGrants;

/// <summary>
/// An append-only ledger row recording that a player was granted Diamond
/// for watching a rewarded ad. The AdMob Server-Side Verification
/// <c>transaction_id</c> is the idempotency key — a unique index
/// guarantees the same verified ad reward grants Diamond at most once.
/// There is no mutable state: corrections are new rows, never edits.
/// </summary>
public class RewardedAdGrant : Entity, IAggregateRoot
{
    public RewardedAdGrantId Id { get; private set; }

    public Guid PlayerId { get; private set; }

    public int DiamondAmount { get; private set; }

    public string TransactionId { get; private set; }

    public DateTime GrantedOn { get; private set; }

    private RewardedAdGrant()
    {
        Id = null!;
        TransactionId = null!;
    }

    private RewardedAdGrant(
        RewardedAdGrantId id,
        Guid playerId,
        int diamondAmount,
        string transactionId,
        DateTime grantedOn)
    {
        Id = id;
        PlayerId = playerId;
        DiamondAmount = diamondAmount;
        TransactionId = transactionId;
        GrantedOn = grantedOn;

        AddDomainEvent(new RewardedAdGrantedDomainEvent(
            id.Value,
            playerId,
            diamondAmount,
            transactionId,
            grantedOn));
    }

    /// <summary>
    /// Records a verified rewarded-ad Diamond grant. The Diamond amount is
    /// the backend-owned value, never the ad-network/client reward value.
    /// </summary>
    public static RewardedAdGrant Create(
        Guid playerId,
        int diamondAmount,
        string transactionId,
        DateTime grantedOn)
    {
        CheckRule(new RewardedAdAmountMustBePositiveRule(diamondAmount));
        CheckRule(new TransactionIdMustNotBeEmptyRule(transactionId));

        return new RewardedAdGrant(
            new RewardedAdGrantId(Guid.NewGuid()),
            playerId,
            diamondAmount,
            transactionId,
            grantedOn);
    }
}
