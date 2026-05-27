namespace LexiLink.Modules.Market.Domain;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByPlayerAndIdempotencyKeyAsync(
        Guid playerId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<int> CountByPlayerAndShopItemAsync(
        Guid playerId,
        ShopItemId shopItemId,
        DateTime? purchasedFrom,
        DateTime? purchasedTo,
        CancellationToken cancellationToken = default);

    Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default);
}
