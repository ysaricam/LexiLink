using LexiLink.Modules.Payments.Application.Configuration.Verification;
using LexiLink.Modules.Payments.Domain;
using Microsoft.Extensions.Options;

namespace LexiLink.Modules.Payments.Infrastructure.Verification.Apple;

internal sealed class AppleIapVerifier : IAppleIapVerifier
{
    private readonly AppleIapOptions _options;

    internal AppleIapVerifier(IOptions<AppleIapOptions> options)
    {
        _options = options.Value;
    }

    public Task<StorePurchaseVerificationResult> VerifyAsync(
        AppleIapVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.BundleId))
        {
            return Task.FromResult(StorePurchaseVerificationResult.Failed(
                PaymentPlatform.Apple,
                _options.Environment,
                request.StoreProductId,
                request.TransactionId,
                purchaseToken: null,
                StorePurchaseState.Invalid,
                "Apple IAP verification is not configured."));
        }

        return Task.FromResult(StorePurchaseVerificationResult.Failed(
            PaymentPlatform.Apple,
            _options.Environment,
            request.StoreProductId,
            request.TransactionId,
            purchaseToken: null,
            StorePurchaseState.Invalid,
            "Apple IAP verifier shell is registered; App Store verification implementation is P4/P5 work."));
    }
}
