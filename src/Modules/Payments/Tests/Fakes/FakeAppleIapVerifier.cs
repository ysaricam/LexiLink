using LexiLink.Modules.Payments.Application.Configuration.Verification;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Tests.Fakes;

public sealed class FakeAppleIapVerifier : IAppleIapVerifier
{
    private readonly Dictionary<string, StorePurchaseVerificationResult> _resultsByTransactionId = new();

    public void AddVerified(
        string transactionId,
        string storeProductId,
        string? accountToken = null,
        string? orderId = null)
    {
        _resultsByTransactionId[transactionId] = StorePurchaseVerificationResult.Verified(
            PaymentPlatform.Apple,
            PaymentEnvironment.Sandbox,
            storeProductId,
            transactionId,
            purchaseToken: null,
            orderId,
            accountToken,
            StorePurchasePostProcessingAction.AppleClientFinishTransaction,
            DateTime.UtcNow);
    }

    public void AddFailure(
        string transactionId,
        string storeProductId,
        StorePurchaseState state,
        string failureReason)
    {
        _resultsByTransactionId[transactionId] = StorePurchaseVerificationResult.Failed(
            PaymentPlatform.Apple,
            PaymentEnvironment.Sandbox,
            storeProductId,
            transactionId,
            purchaseToken: null,
            state,
            failureReason);
    }

    public Task<StorePurchaseVerificationResult> VerifyAsync(
        AppleIapVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_resultsByTransactionId.TryGetValue(request.TransactionId, out var result))
        {
            return Task.FromResult(result);
        }

        return Task.FromResult(StorePurchaseVerificationResult.Failed(
            PaymentPlatform.Apple,
            PaymentEnvironment.Sandbox,
            request.StoreProductId,
            request.TransactionId,
            purchaseToken: null,
            StorePurchaseState.Invalid,
            "Fake Apple verifier has no result for the transaction id."));
    }
}
