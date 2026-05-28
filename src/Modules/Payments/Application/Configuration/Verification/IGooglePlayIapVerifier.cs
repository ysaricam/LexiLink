namespace LexiLink.Modules.Payments.Application.Configuration.Verification;

public interface IGooglePlayIapVerifier
{
    Task<StorePurchaseVerificationResult> VerifyAsync(
        GooglePlayIapVerificationRequest request,
        CancellationToken cancellationToken = default);
}
