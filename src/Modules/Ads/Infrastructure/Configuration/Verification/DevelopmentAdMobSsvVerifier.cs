using LexiLink.Modules.Ads.Application.Configuration.Verification;

namespace LexiLink.Modules.Ads.Infrastructure.Configuration.Verification;

/// <summary>
/// Fail-open development verifier: accepts any callback so the rewarded-ad
/// grant path can be exercised locally. Google's real SSV servers cannot
/// reach <c>localhost</c>, so signature verification is impossible in dev;
/// this verifier trusts the caller. It is selected only when
/// <c>Ads:Ssv:Mode = DevelopmentFailOpen</c> and must never run in
/// production, where the fail-closed <see cref="AdMobSsvVerifier"/> applies.
/// </summary>
public sealed class DevelopmentAdMobSsvVerifier : IAdMobSsvVerifier
{
    public Task<AdMobSsvVerificationResult> VerifyAsync(
        AdMobSsvVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(AdMobSsvVerificationResult.Verified());
    }
}
