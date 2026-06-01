namespace LexiLink.Modules.Ads.Application.Configuration.Verification;

/// <summary>
/// Verifies an AdMob Server-Side Verification (SSV) rewarded-ad callback.
/// The signature is computed by Google over the query-string content
/// preceding the <c>signature</c> parameter, using a rotating ECDSA key
/// identified by <c>key_id</c>. The real implementation lives behind a
/// fail-closed infrastructure shell until verification keys are wired; a
/// fail-open development verifier enables local testing because Google
/// cannot reach <c>localhost</c>.
/// </summary>
public interface IAdMobSsvVerifier
{
    Task<AdMobSsvVerificationResult> VerifyAsync(
        AdMobSsvVerificationRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>The signed material plus the signature/key needed to verify it.</summary>
public sealed record AdMobSsvVerificationRequest(
    string SignedContent,
    string Signature,
    string KeyId,
    string TransactionId,
    string UserId);

public sealed record AdMobSsvVerificationResult(bool IsVerified, string? FailureReason)
{
    public static AdMobSsvVerificationResult Verified() => new(true, null);

    public static AdMobSsvVerificationResult Failed(string reason) => new(false, reason);
}

public enum AdsSsvVerificationMode
{
    /// <summary>Real signature verification; fails closed until configured.</summary>
    Production,

    /// <summary>Fail-open for local testing — Google cannot reach localhost.</summary>
    DevelopmentFailOpen
}

public sealed class AdsSsvOptions
{
    public const string SectionName = "Ads:Ssv";

    public AdsSsvVerificationMode Mode { get; set; } = AdsSsvVerificationMode.Production;

    public string VerificationKeysUrl { get; set; } =
        "https://www.gstatic.com/admob/reward/verifier-keys.json";
}
