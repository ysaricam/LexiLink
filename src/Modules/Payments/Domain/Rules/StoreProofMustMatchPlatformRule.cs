using LexiLink.Common.Domain;

namespace LexiLink.Modules.Payments.Domain.Rules;

internal sealed class StoreProofMustMatchPlatformRule : IBusinessRule
{
    private readonly PaymentPlatform _platform;
    private readonly StoreTransactionId? _storeTransactionId;
    private readonly PurchaseToken? _purchaseToken;

    internal StoreProofMustMatchPlatformRule(
        PaymentPlatform platform,
        StoreTransactionId? storeTransactionId,
        PurchaseToken? purchaseToken)
    {
        _platform = platform;
        _storeTransactionId = storeTransactionId;
        _purchaseToken = purchaseToken;
    }

    public bool IsBroken() =>
        _platform switch
        {
            PaymentPlatform.Apple => _storeTransactionId is null,
            PaymentPlatform.Google => _purchaseToken is null,
            _ => true
        };

    public string Message => "Store proof must match the payment platform.";
}
