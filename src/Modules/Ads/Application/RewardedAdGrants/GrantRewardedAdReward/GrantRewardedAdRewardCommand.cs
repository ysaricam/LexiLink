using LexiLink.Modules.Ads.Application.Contracts;

namespace LexiLink.Modules.Ads.Application.RewardedAdGrants.GrantRewardedAdReward;

/// <summary>
/// Processes a single AdMob SSV rewarded-ad callback: verifies the
/// signature, enforces idempotency on <see cref="TransactionId"/> and the
/// per-player daily cap, then grants the backend-owned Diamond amount.
/// </summary>
public sealed class GrantRewardedAdRewardCommand : CommandBase<GrantRewardedAdRewardResultDto>
{
    public GrantRewardedAdRewardCommand(
        string userId,
        string transactionId,
        string adUnitId,
        string? customData,
        int rewardAmount,
        string? rewardItem,
        string keyId,
        string signature,
        string signedContent)
    {
        UserId = userId;
        TransactionId = transactionId;
        AdUnitId = adUnitId;
        CustomData = customData;
        RewardAmount = rewardAmount;
        RewardItem = rewardItem;
        KeyId = keyId;
        Signature = signature;
        SignedContent = signedContent;
    }

    /// <summary>AdMob SSV <c>user_id</c> — the player id the client passed.</summary>
    public string UserId { get; }

    /// <summary>AdMob SSV <c>transaction_id</c> — the idempotency key.</summary>
    public string TransactionId { get; }

    public string AdUnitId { get; }

    public string? CustomData { get; }

    /// <summary>Ad-network reward value — recorded for audit, never granted.</summary>
    public int RewardAmount { get; }

    public string? RewardItem { get; }

    public string KeyId { get; }

    public string Signature { get; }

    /// <summary>The query-string content the AdMob signature covers.</summary>
    public string SignedContent { get; }
}
