using FluentValidation;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.IapPurchases.VerifyIapPurchase;

internal sealed class VerifyIapPurchaseCommandValidator : AbstractValidator<VerifyIapPurchaseCommand>
{
    public VerifyIapPurchaseCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.StoreProductId).NotEmpty().MaximumLength(128);
        RuleFor(x => x.ClientRequestId).MaximumLength(128);
        RuleFor(x => x.StoreTransactionId).MaximumLength(256);
        RuleFor(x => x.PurchaseToken).MaximumLength(2048);

        When(x => x.Platform == PaymentPlatform.Apple, () =>
        {
            RuleFor(x => x.StoreTransactionId).NotEmpty();
        });

        When(x => x.Platform == PaymentPlatform.Google, () =>
        {
            RuleFor(x => x.PurchaseToken).NotEmpty();
        });
    }
}
