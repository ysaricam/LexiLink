using LexiLink.Modules.Market.Domain;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Market.Infrastructure.Domain.PurchaseOrders;

internal class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly MarketContext _marketContext;

    internal PurchaseOrderRepository(MarketContext marketContext)
    {
        _marketContext = marketContext;
    }

    public async Task<PurchaseOrder?> GetByIdAsync(PurchaseOrderId id, CancellationToken cancellationToken = default)
    {
        return await _marketContext.PurchaseOrders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<PurchaseOrder?> GetByPlayerAndIdempotencyKeyAsync(
        Guid playerId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return await _marketContext.PurchaseOrders.FirstOrDefaultAsync(
            x => EF.Property<Guid>(x, "_playerId") == playerId
                 && EF.Property<string>(x, "_idempotencyKey") == idempotencyKey,
            cancellationToken);
    }

    public Task<int> CountByPlayerAndShopItemAsync(
        Guid playerId,
        ShopItemId shopItemId,
        DateTime? purchasedFrom,
        DateTime? purchasedTo,
        CancellationToken cancellationToken = default)
    {
        var query = _marketContext.PurchaseOrders.Where(
            x => EF.Property<Guid>(x, "_playerId") == playerId
                 && EF.Property<ShopItemId>(x, "_shopItemId") == shopItemId);

        if (purchasedFrom is not null)
        {
            query = query.Where(x => EF.Property<DateTime>(x, "_purchasedAt") >= purchasedFrom.Value);
        }

        if (purchasedTo is not null)
        {
            query = query.Where(x => EF.Property<DateTime>(x, "_purchasedAt") < purchasedTo.Value);
        }

        return query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
    {
        await _marketContext.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);
    }
}
