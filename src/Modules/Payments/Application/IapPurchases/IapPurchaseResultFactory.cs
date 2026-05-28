using LexiLink.Modules.Payments.Application.IapPurchases.VerifyIapPurchase;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.IapPurchases;

internal static class IapPurchaseResultFactory
{
    internal static VerifyIapPurchaseResultDto ToVerifyResult(IapPurchase purchase, bool isReplay) =>
        new(
            purchase.Id.Value,
            purchase.StoreProductId.Value,
            purchase.DiamondAmount,
            purchase.Status.ToString(),
            purchase.PostProcessingAction.ToString(),
            purchase.PostProcessingStatus.ToString(),
            purchase.Status == IapPurchaseStatus.Granted &&
                purchase.PostProcessingAction == IapPurchasePostProcessingAction.AppleClientFinishTransaction &&
                purchase.PostProcessingStatus == IapPurchasePostProcessingStatus.Succeeded,
            purchase.PostProcessingFailureReason,
            isReplay);
}
