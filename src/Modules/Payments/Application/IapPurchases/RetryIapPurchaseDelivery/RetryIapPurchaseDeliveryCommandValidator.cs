using FluentValidation;

namespace LexiLink.Modules.Payments.Application.IapPurchases.RetryIapPurchaseDelivery;

internal sealed class RetryIapPurchaseDeliveryCommandValidator
    : AbstractValidator<RetryIapPurchaseDeliveryCommand>
{
    public RetryIapPurchaseDeliveryCommandValidator()
    {
        RuleFor(x => x.IapPurchaseId).NotEmpty();
    }
}
