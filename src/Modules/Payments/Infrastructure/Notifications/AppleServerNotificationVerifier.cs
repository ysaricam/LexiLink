using LexiLink.Modules.Payments.Application.Configuration.Notifications;
using LexiLink.Modules.Payments.Application.Configuration.Verification;
using LexiLink.Modules.Payments.Domain;
using Microsoft.Extensions.Options;

namespace LexiLink.Modules.Payments.Infrastructure.Notifications;

internal sealed class AppleServerNotificationVerifier : IAppleServerNotificationVerifier
{
    private readonly AppleIapOptions _options;

    internal AppleServerNotificationVerifier(IOptions<AppleIapOptions> options)
    {
        _options = options.Value;
    }

    public Task<PaymentNotificationVerificationResult> VerifyAsync(
        AppleServerNotificationVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.BundleId))
        {
            return Task.FromResult(PaymentNotificationVerificationResult.Failed(
                PaymentPlatform.Apple,
                _options.Environment,
                notificationId: Guid.NewGuid().ToString(),
                notificationType: "UNVERIFIED",
                payloadJson: request.SignedPayload,
                "Apple server notification verification is not configured."));
        }

        return Task.FromResult(PaymentNotificationVerificationResult.Failed(
            PaymentPlatform.Apple,
            _options.Environment,
            notificationId: Guid.NewGuid().ToString(),
            notificationType: "UNVERIFIED",
            payloadJson: request.SignedPayload,
            "Apple server notification verifier shell is registered; App Store Server Notifications V2 signature verification is not implemented."));
    }
}
