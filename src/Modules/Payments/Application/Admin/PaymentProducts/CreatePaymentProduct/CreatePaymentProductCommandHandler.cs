using LexiLink.Common.Domain;
using LexiLink.Modules.Payments.Application.Configuration.Commands;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.Admin.PaymentProducts.CreatePaymentProduct;

internal sealed class CreatePaymentProductCommandHandler
    : ICommandHandler<CreatePaymentProductCommand, Guid>
{
    private readonly IPaymentProductRepository _repository;

    internal CreatePaymentProductCommandHandler(IPaymentProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(
        CreatePaymentProductCommand request,
        CancellationToken cancellationToken)
    {
        var storeProductId = StoreProductId.Of(request.StoreProductId);
        var existing = await _repository.GetByStoreProductIdAsync(storeProductId, cancellationToken);
        if (existing is not null)
        {
            throw new BusinessRuleValidationException(
                new StoreProductIdMustBeUniqueRule(storeProductId.Value));
        }

        var product = PaymentProduct.Create(
            storeProductId,
            request.DiamondAmount,
            request.IsAppleAvailable,
            request.IsGoogleAvailable,
            request.SortOrder);

        await _repository.AddAsync(product, cancellationToken);
        return product.Id.Value;
    }

    private sealed class StoreProductIdMustBeUniqueRule : IBusinessRule
    {
        private readonly string _storeProductId;

        public StoreProductIdMustBeUniqueRule(string storeProductId)
        {
            _storeProductId = storeProductId;
        }

        public bool IsBroken() => true;
        public string Message => $"Payment product '{_storeProductId}' already exists.";
    }
}
