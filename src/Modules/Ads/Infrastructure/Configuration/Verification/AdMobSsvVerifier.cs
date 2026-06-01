using LexiLink.Modules.Ads.Application.Configuration.Verification;

namespace LexiLink.Modules.Ads.Infrastructure.Configuration.Verification;

/// <summary>
/// Fail-closed SSV verifier shell. Real AdMob signature verification —
/// fetching Google's rotating ECDSA public keys from
/// <see cref="AdsSsvOptions.VerificationKeysUrl"/>, selecting by
/// <c>key_id</c>, and verifying the signature over the signed content —
/// is operator/credential work deferred beyond AD2. Until then every
/// callback is rejected so no Diamond can leak from an unverified reward.
/// </summary>
public sealed class AdMobSsvVerifier : IAdMobSsvVerifier
{
    private readonly AdsSsvOptions _options;

    public AdMobSsvVerifier(AdsSsvOptions options)
    {
        _options = options;
    }

    public Task<AdMobSsvVerificationResult> VerifyAsync(
        AdMobSsvVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(AdMobSsvVerificationResult.Failed(
            "AdMob SSV verifier shell is registered; real signature verification " +
            "against Google's public keys is not yet implemented (fail-closed)."));
    }
}
