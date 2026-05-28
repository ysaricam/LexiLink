using LexiLink.Modules.Payments.Application.Contracts;
using LexiLink.Modules.Payments.Application.IapPurchases.VerifyIapPurchase;

namespace LexiLink.Modules.Payments.Application.IapPurchases.RetryIapPurchaseDelivery;

public sealed class RetryIapPurchaseDeliveryCommand : CommandBase<VerifyIapPurchaseResultDto>
{
    public Guid IapPurchaseId { get; }

    public RetryIapPurchaseDeliveryCommand(Guid iapPurchaseId)
    {
        IapPurchaseId = iapPurchaseId;
    }
}
