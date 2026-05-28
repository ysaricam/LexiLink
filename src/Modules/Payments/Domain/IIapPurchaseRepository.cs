namespace LexiLink.Modules.Payments.Domain;

public interface IIapPurchaseRepository
{
    Task<IapPurchase?> GetByIdAsync(
        IapPurchaseId id,
        CancellationToken cancellationToken = default);

    Task<IapPurchase?> GetByStoreTransactionIdAsync(
        PaymentPlatform platform,
        StoreTransactionId storeTransactionId,
        CancellationToken cancellationToken = default);

    Task<IapPurchase?> GetByPurchaseTokenAsync(
        PaymentPlatform platform,
        PurchaseToken purchaseToken,
        CancellationToken cancellationToken = default);

    Task<IapPurchase?> GetByPlayerAndClientRequestIdAsync(
        Guid playerId,
        string clientRequestId,
        CancellationToken cancellationToken = default);

    Task AddAsync(IapPurchase purchase, CancellationToken cancellationToken = default);
}
