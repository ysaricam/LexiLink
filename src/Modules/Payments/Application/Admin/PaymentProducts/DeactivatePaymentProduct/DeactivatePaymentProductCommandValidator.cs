using FluentValidation;

namespace LexiLink.Modules.Payments.Application.Admin.PaymentProducts.DeactivatePaymentProduct;

internal sealed class DeactivatePaymentProductCommandValidator
    : AbstractValidator<DeactivatePaymentProductCommand>
{
    public DeactivatePaymentProductCommandValidator()
    {
        RuleFor(x => x.PaymentProductId).NotEmpty();
    }
}
