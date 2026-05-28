using LexiLink.Modules.Payments.Application.Contracts;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.IapPurchases.VerifyIapPurchase;

public sealed class VerifyIapPurchaseCommand : CommandBase<VerifyIapPurchaseResultDto>
{
    public Guid PlayerId { get; }
    public PaymentPlatform Platform { get; }
    public string StoreProductId { get; }
    public string? StoreTransactionId { get; }
    public string? PurchaseToken { get; }
    public string? SignedTransactionJws { get; }
    public string? AccountToken { get; }
    public string? ClientRequestId { get; }

    public VerifyIapPurchaseCommand(
        Guid playerId,
        PaymentPlatform platform,
        string storeProductId,
        string? storeTransactionId,
        string? purchaseToken,
        string? signedTransactionJws,
        string? accountToken,
        string? clientRequestId)
    {
        PlayerId = playerId;
        Platform = platform;
        StoreProductId = storeProductId;
        StoreTransactionId = storeTransactionId;
        PurchaseToken = purchaseToken;
        SignedTransactionJws = signedTransactionJws;
        AccountToken = accountToken;
        ClientRequestId = clientRequestId;
    }
}
