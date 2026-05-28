using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Payments.Application.Configuration.Commands;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.Admin.PaymentProducts.DeactivatePaymentProduct;

internal sealed class DeactivatePaymentProductCommandHandler
    : ICommandHandler<DeactivatePaymentProductCommand>
{
    private readonly IPaymentProductRepository _repository;

    internal DeactivatePaymentProductCommandHandler(IPaymentProductRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        DeactivatePaymentProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(
                new PaymentProductId(request.PaymentProductId),
                cancellationToken)
            ?? throw new NotFoundException(nameof(PaymentProduct), request.PaymentProductId);

        product.Deactivate();
    }
}
