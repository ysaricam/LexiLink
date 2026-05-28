using LexiLink.Modules.Payments.Application.Configuration.Verification;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.IapPurchases;

internal static class IapPurchasePostProcessing
{
    internal static async Task ProcessAsync(
        IapPurchase purchase,
        IGooglePlayPurchaseProcessor googleProcessor,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (purchase.PostProcessingAction is IapPurchasePostProcessingAction.None)
        {
            return;
        }

        if (purchase.PostProcessingAction is IapPurchasePostProcessingAction.AppleClientFinishTransaction)
        {
            purchase.MarkPostProcessingSucceeded(now);
            return;
        }

        if (purchase.PurchaseToken is null)
        {
            purchase.MarkPostProcessingFailed("Google purchase token is missing.");
            return;
        }

        var result = purchase.PostProcessingAction switch
        {
            IapPurchasePostProcessingAction.GoogleAcknowledge =>
                await googleProcessor.AcknowledgeAsync(
                    purchase.StoreProductId.Value,
                    purchase.PurchaseToken.Value,
                    cancellationToken),
            IapPurchasePostProcessingAction.GoogleConsume =>
                await googleProcessor.ConsumeAsync(
                    purchase.StoreProductId.Value,
                    purchase.PurchaseToken.Value,
                    cancellationToken),
            _ => GooglePlayPostProcessingResult.Failed(
                $"Unsupported post-processing action '{purchase.PostProcessingAction}'.")
        };

        if (result.Succeeded)
        {
            purchase.MarkPostProcessingSucceeded(now);
            return;
        }

        purchase.MarkPostProcessingFailed(
            result.FailureReason ?? "Store post-processing failed.");
    }

    internal static IapPurchasePostProcessingAction MapAction(
        StorePurchasePostProcessingAction action) =>
        action switch
        {
            StorePurchasePostProcessingAction.None => IapPurchasePostProcessingAction.None,
            StorePurchasePostProcessingAction.AppleClientFinishTransaction =>
                IapPurchasePostProcessingAction.AppleClientFinishTransaction,
            StorePurchasePostProcessingAction.GoogleAcknowledge =>
                IapPurchasePostProcessingAction.GoogleAcknowledge,
            StorePurchasePostProcessingAction.GoogleConsume =>
                IapPurchasePostProcessingAction.GoogleConsume,
            _ => IapPurchasePostProcessingAction.None
        };
}
