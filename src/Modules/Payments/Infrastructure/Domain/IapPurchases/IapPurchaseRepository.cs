using LexiLink.Modules.Payments.Domain;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Payments.Infrastructure.Domain.IapPurchases;

internal class IapPurchaseRepository : IIapPurchaseRepository
{
    private readonly PaymentsContext _context;

    internal IapPurchaseRepository(PaymentsContext context)
    {
        _context = context;
    }

    public Task<IapPurchase?> GetByIdAsync(
        IapPurchaseId id,
        CancellationToken cancellationToken = default) =>
        _context.IapPurchases.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<IapPurchase?> GetByStoreTransactionIdAsync(
        PaymentPlatform platform,
        StoreTransactionId storeTransactionId,
        CancellationToken cancellationToken = default) =>
        _context.IapPurchases
            .FromSqlInterpolated($"""
                SELECT *
                FROM payments."IapPurchases"
                WHERE "Platform" = {(int)platform}
                AND "StoreTransactionId" = {storeTransactionId.Value}
                LIMIT 1
                """)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<IapPurchase?> GetByPurchaseTokenAsync(
        PaymentPlatform platform,
        PurchaseToken purchaseToken,
        CancellationToken cancellationToken = default) =>
        _context.IapPurchases
            .FromSqlInterpolated($"""
                SELECT *
                FROM payments."IapPurchases"
                WHERE "Platform" = {(int)platform}
                AND "PurchaseToken" = {purchaseToken.Value}
                LIMIT 1
                """)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<IapPurchase?> GetByPlayerAndClientRequestIdAsync(
        Guid playerId,
        string clientRequestId,
        CancellationToken cancellationToken = default) =>
        _context.IapPurchases
            .FromSqlInterpolated($"""
                SELECT *
                FROM payments."IapPurchases"
                WHERE "PlayerId" = {playerId}
                AND "ClientRequestId" = {clientRequestId}
                LIMIT 1
                """)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(IapPurchase purchase, CancellationToken cancellationToken = default) =>
        await _context.IapPurchases.AddAsync(purchase, cancellationToken);
}
