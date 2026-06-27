using LexiLink.Modules.Payments.Domain;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Payments.Infrastructure.Domain.PaymentProducts;

internal class PaymentProductRepository : IPaymentProductRepository
{
    private readonly PaymentsContext _context;

    internal PaymentProductRepository(PaymentsContext context)
    {
        _context = context;
    }

    public Task<PaymentProduct?> GetByIdAsync(
        PaymentProductId id,
        CancellationToken cancellationToken = default) =>
        _context.PaymentProducts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<PaymentProduct?> GetByStoreProductIdAsync(
        StoreProductId storeProductId,
        CancellationToken cancellationToken = default) =>
        _context.PaymentProducts
            .FromSqlInterpolated($"""
                SELECT *
                FROM payments."PaymentProducts"
                WHERE "StoreProductId" = {storeProductId.Value}
                LIMIT 1
                """)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(PaymentProduct product, CancellationToken cancellationToken = default) =>
        await _context.PaymentProducts.AddAsync(product, cancellationToken);
}
