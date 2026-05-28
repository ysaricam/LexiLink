using LexiLink.Modules.Payments.Application.Configuration.Verification;
using Microsoft.Extensions.Options;

namespace LexiLink.Modules.Payments.Infrastructure.Verification.Google;

internal sealed class GooglePlayPurchaseProcessor : IGooglePlayPurchaseProcessor
{
    private readonly GooglePlayIapOptions _options;

    internal GooglePlayPurchaseProcessor(IOptions<GooglePlayIapOptions> options)
    {
        _options = options.Value;
    }

    public Task<GooglePlayPostProcessingResult> AcknowledgeAsync(
        string storeProductId,
        string purchaseToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.PackageName))
        {
            return Task.FromResult(GooglePlayPostProcessingResult.Failed(
                "Google Play post-processing is not configured."));
        }

        return Task.FromResult(GooglePlayPostProcessingResult.Failed(
            "Google Play acknowledge shell is registered; Play Developer API implementation is not configured."));
    }

    public Task<GooglePlayPostProcessingResult> ConsumeAsync(
        string storeProductId,
        string purchaseToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.PackageName))
        {
            return Task.FromResult(GooglePlayPostProcessingResult.Failed(
                "Google Play post-processing is not configured."));
        }

        return Task.FromResult(GooglePlayPostProcessingResult.Failed(
            "Google Play consume shell is registered; Play Developer API implementation is not configured."));
    }
}
