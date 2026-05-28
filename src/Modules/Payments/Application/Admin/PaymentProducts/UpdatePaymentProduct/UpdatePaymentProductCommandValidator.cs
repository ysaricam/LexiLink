using FluentValidation;

namespace LexiLink.Modules.Payments.Application.Admin.PaymentProducts.UpdatePaymentProduct;

internal sealed class UpdatePaymentProductCommandValidator
    : AbstractValidator<UpdatePaymentProductCommand>
{
    public UpdatePaymentProductCommandValidator()
    {
        RuleFor(x => x.PaymentProductId).NotEmpty();
        RuleFor(x => x.DiamondAmount).GreaterThan(0);
        RuleFor(x => new { x.IsAppleAvailable, x.IsGoogleAvailable })
            .Must(x => x.IsAppleAvailable || x.IsGoogleAvailable)
            .WithMessage("Payment product must be available on at least one platform.");
    }
}
