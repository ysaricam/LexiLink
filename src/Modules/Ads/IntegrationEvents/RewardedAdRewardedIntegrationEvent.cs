using LexiLink.Common.Application.IntegrationEvents;

namespace LexiLink.Modules.Ads.IntegrationEvents;

/// <summary>
/// Published after a rewarded-ad Diamond grant is verified and recorded.
/// Downstream (BI/audit) signal; Diamond is granted synchronously via
/// <c>IDiamondGrant</c>, not through this event.
/// </summary>
public sealed record RewardedAdRewardedIntegrationEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid RewardedAdGrantId,
    Guid PlayerId,
    int DiamondAmount,
    string TransactionId,
    DateTime GrantedOn) : IIntegrationEvent;
