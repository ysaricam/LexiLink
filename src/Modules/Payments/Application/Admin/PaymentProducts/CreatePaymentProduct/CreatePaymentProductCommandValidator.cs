using FluentValidation;

namespace LexiLink.Modules.Payments.Application.Admin.PaymentProducts.CreatePaymentProduct;

internal sealed class CreatePaymentProductCommandValidator
    : AbstractValidator<CreatePaymentProductCommand>
{
    public CreatePaymentProductCommandValidator()
    {
        RuleFor(x => x.StoreProductId).NotEmpty().MaximumLength(128);
        RuleFor(x => x.DiamondAmount).GreaterThan(0);
        RuleFor(x => new { x.IsAppleAvailable, x.IsGoogleAvailable })
            .Must(x => x.IsAppleAvailable || x.IsGoogleAvailable)
            .WithMessage("Payment product must be available on at least one platform.");
    }
}
