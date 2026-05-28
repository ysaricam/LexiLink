using LexiLink.Modules.Payments.Application.Configuration.Verification;
using LexiLink.Modules.Payments.Domain;
using Microsoft.Extensions.Options;

namespace LexiLink.Modules.Payments.Infrastructure.Verification.Google;

internal sealed class GooglePlayIapVerifier : IGooglePlayIapVerifier
{
    private readonly GooglePlayIapOptions _options;

    internal GooglePlayIapVerifier(IOptions<GooglePlayIapOptions> options)
    {
        _options = options.Value;
    }

    public Task<StorePurchaseVerificationResult> VerifyAsync(
        GooglePlayIapVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.PackageName))
        {
            return Task.FromResult(StorePurchaseVerificationResult.Failed(
                PaymentPlatform.Google,
                _options.Environment,
                request.StoreProductId,
                storeTransactionId: null,
                request.PurchaseToken,
                StorePurchaseState.Invalid,
                "Google Play IAP verification is not configured."));
        }

        return Task.FromResult(StorePurchaseVerificationResult.Failed(
            PaymentPlatform.Google,
            _options.Environment,
            request.StoreProductId,
            storeTransactionId: null,
            request.PurchaseToken,
            StorePurchaseState.Invalid,
            "Google Play verifier shell is registered; Play Developer API verification implementation is P4/P5 work."));
    }
}
