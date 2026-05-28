namespace LexiLink.Modules.Payments.Application.Configuration.Verification;

public interface IGooglePlayPurchaseProcessor
{
    Task<GooglePlayPostProcessingResult> AcknowledgeAsync(
        string storeProductId,
        string purchaseToken,
        CancellationToken cancellationToken = default);

    Task<GooglePlayPostProcessingResult> ConsumeAsync(
        string storeProductId,
        string purchaseToken,
        CancellationToken cancellationToken = default);
}

public sealed record GooglePlayPostProcessingResult(
    bool Succeeded,
    bool IsReplay,
    string? FailureReason)
{
    public static GooglePlayPostProcessingResult Success(bool isReplay = false) =>
        new(true, isReplay, FailureReason: null);

    public static GooglePlayPostProcessingResult Failed(string failureReason) =>
        new(false, IsReplay: false, failureReason);
}
