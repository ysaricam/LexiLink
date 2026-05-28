using LexiLink.Modules.Payments.Application.Configuration.Verification;

namespace LexiLink.Modules.Payments.Tests.Fakes;

public sealed class FakeGooglePlayPurchaseProcessor : IGooglePlayPurchaseProcessor
{
    private readonly HashSet<string> _acknowledgedTokens = [];
    private readonly HashSet<string> _consumedTokens = [];

    public bool FailAcknowledge { get; set; }
    public bool FailConsume { get; set; }
    public IReadOnlyCollection<string> AcknowledgedTokens => _acknowledgedTokens;
    public IReadOnlyCollection<string> ConsumedTokens => _consumedTokens;

    public Task<GooglePlayPostProcessingResult> AcknowledgeAsync(
        string storeProductId,
        string purchaseToken,
        CancellationToken cancellationToken = default)
    {
        if (FailAcknowledge)
        {
            return Task.FromResult(GooglePlayPostProcessingResult.Failed("Acknowledge failed."));
        }

        var isReplay = !_acknowledgedTokens.Add(purchaseToken);
        return Task.FromResult(GooglePlayPostProcessingResult.Success(isReplay));
    }

    public Task<GooglePlayPostProcessingResult> ConsumeAsync(
        string storeProductId,
        string purchaseToken,
        CancellationToken cancellationToken = default)
    {
        if (FailConsume)
        {
            return Task.FromResult(GooglePlayPostProcessingResult.Failed("Consume failed."));
        }

        var isReplay = !_consumedTokens.Add(purchaseToken);
        return Task.FromResult(GooglePlayPostProcessingResult.Success(isReplay));
    }
}
