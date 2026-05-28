using LexiLink.Modules.Payments.Application.Configuration.Notifications;
using LexiLink.Modules.Payments.Application.Configuration.Verification;
using LexiLink.Modules.Payments.Domain;
using Microsoft.Extensions.Options;

namespace LexiLink.Modules.Payments.Infrastructure.Notifications;

internal sealed class GoogleRealTimeDeveloperNotificationVerifier
    : IGoogleRealTimeDeveloperNotificationVerifier
{
    private readonly GooglePlayIapOptions _options;

    internal GoogleRealTimeDeveloperNotificationVerifier(IOptions<GooglePlayIapOptions> options)
    {
        _options = options.Value;
    }

    public Task<PaymentNotificationVerificationResult> VerifyAsync(
        GoogleRealTimeDeveloperNotificationVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.PackageName))
        {
            return Task.FromResult(PaymentNotificationVerificationResult.Failed(
                PaymentPlatform.Google,
                _options.Environment,
                request.MessageId,
                notificationType: "UNVERIFIED",
                request.PayloadJson,
                "Google RTDN verification is not configured."));
        }

        return Task.FromResult(PaymentNotificationVerificationResult.Failed(
            PaymentPlatform.Google,
            _options.Environment,
            request.MessageId,
            notificationType: "UNVERIFIED",
            request.PayloadJson,
            "Google RTDN verifier shell is registered; Pub/Sub token and RTDN payload verification is not implemented."));
    }
}
