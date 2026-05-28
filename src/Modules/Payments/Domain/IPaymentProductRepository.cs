namespace LexiLink.Modules.Payments.Domain;

public interface IPaymentProductRepository
{
    Task<PaymentProduct?> GetByIdAsync(
        PaymentProductId id,
        CancellationToken cancellationToken = default);

    Task<PaymentProduct?> GetByStoreProductIdAsync(
        StoreProductId storeProductId,
        CancellationToken cancellationToken = default);

    Task AddAsync(PaymentProduct product, CancellationToken cancellationToken = default);
}
