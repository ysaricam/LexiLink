using LexiLink.Modules.Payments.Application.Configuration.Verification;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Tests.Fakes;

public sealed class FakeGooglePlayIapVerifier : IGooglePlayIapVerifier
{
    private readonly Dictionary<string, StorePurchaseVerificationResult> _resultsByPurchaseToken = new();

    public void AddVerified(
        string purchaseToken,
        string storeProductId,
        string? accountToken = null,
        string? orderId = null)
    {
        _resultsByPurchaseToken[purchaseToken] = StorePurchaseVerificationResult.Verified(
            PaymentPlatform.Google,
            PaymentEnvironment.Sandbox,
            storeProductId,
            storeTransactionId: null,
            purchaseToken,
            orderId,
            accountToken,
            StorePurchasePostProcessingAction.GoogleConsume,
            DateTime.UtcNow);
    }

    public void AddFailure(
        string purchaseToken,
        string storeProductId,
        StorePurchaseState state,
        string failureReason)
    {
        _resultsByPurchaseToken[purchaseToken] = StorePurchaseVerificationResult.Failed(
            PaymentPlatform.Google,
            PaymentEnvironment.Sandbox,
            storeProductId,
            storeTransactionId: null,
            purchaseToken,
            state,
            failureReason);
    }

    public Task<StorePurchaseVerificationResult> VerifyAsync(
        GooglePlayIapVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_resultsByPurchaseToken.TryGetValue(request.PurchaseToken, out var result))
        {
            return Task.FromResult(result);
        }

        return Task.FromResult(StorePurchaseVerificationResult.Failed(
            PaymentPlatform.Google,
            PaymentEnvironment.Sandbox,
            request.StoreProductId,
            storeTransactionId: null,
            request.PurchaseToken,
            StorePurchaseState.Invalid,
            "Fake Google Play verifier has no result for the purchase token."));
    }
}
