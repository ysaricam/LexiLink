using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Payments.Application.Configuration.Commands;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.Admin.PaymentProducts.UpdatePaymentProduct;

internal sealed class UpdatePaymentProductCommandHandler : ICommandHandler<UpdatePaymentProductCommand>
{
    private readonly IPaymentProductRepository _repository;

    internal UpdatePaymentProductCommandHandler(IPaymentProductRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(UpdatePaymentProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(
                new PaymentProductId(request.PaymentProductId),
                cancellationToken)
            ?? throw new NotFoundException(nameof(PaymentProduct), request.PaymentProductId);

        product.Update(
            request.DiamondAmount,
            request.IsAppleAvailable,
            request.IsGoogleAvailable,
            request.SortOrder);
    }
}
