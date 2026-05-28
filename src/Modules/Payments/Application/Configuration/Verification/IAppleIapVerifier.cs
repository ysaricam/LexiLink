namespace LexiLink.Modules.Payments.Application.Configuration.Verification;

public interface IAppleIapVerifier
{
    Task<StorePurchaseVerificationResult> VerifyAsync(
        AppleIapVerificationRequest request,
        CancellationToken cancellationToken = default);
}
