using LexiLink.Modules.Payments.Application.Configuration.Commands;
using LexiLink.Modules.Payments.Application.Configuration.Notifications;
using LexiLink.Modules.Payments.Domain;
using LexiLink.Common.Application.Time;

namespace LexiLink.Modules.Payments.Application.Notifications.ReceivePaymentNotification;

internal sealed class ReceiveApplePaymentNotificationCommandHandler
    : ICommandHandler<ReceiveApplePaymentNotificationCommand, PaymentNotificationResultDto>
{
    private readonly IAppleServerNotificationVerifier _verifier;
    private readonly ReceivePaymentNotificationProcessor _processor;

    internal ReceiveApplePaymentNotificationCommandHandler(
        IAppleServerNotificationVerifier verifier,
        ReceivePaymentNotificationProcessor processor)
    {
        _verifier = verifier;
        _processor = processor;
    }

    public async Task<PaymentNotificationResultDto> Handle(
        ReceiveApplePaymentNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var verification = await _verifier.VerifyAsync(
            new AppleServerNotificationVerificationRequest(request.SignedPayload),
            cancellationToken);

        return await _processor.ProcessAsync(verification, cancellationToken);
    }
}

internal sealed class ReceiveGooglePaymentNotificationCommandHandler
    : ICommandHandler<ReceiveGooglePaymentNotificationCommand, PaymentNotificationResultDto>
{
    private readonly IGoogleRealTimeDeveloperNotificationVerifier _verifier;
    private readonly ReceivePaymentNotificationProcessor _processor;

    internal ReceiveGooglePaymentNotificationCommandHandler(
        IGoogleRealTimeDeveloperNotificationVerifier verifier,
        ReceivePaymentNotificationProcessor processor)
    {
        _verifier = verifier;
        _processor = processor;
    }

    public async Task<PaymentNotificationResultDto> Handle(
        ReceiveGooglePaymentNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var verification = await _verifier.VerifyAsync(
            new GoogleRealTimeDeveloperNotificationVerificationRequest(
                request.MessageId,
                request.PayloadJson,
                request.AuthorizationToken),
            cancellationToken);

        return await _processor.ProcessAsync(verification, cancellationToken);
    }
}

internal sealed class ReceivePaymentNotificationProcessor
{
    private readonly IPaymentNotificationRepository _notificationRepository;
    private readonly IIapPurchaseRepository _purchaseRepository;
    private readonly IClock _clock;

    internal ReceivePaymentNotificationProcessor(
        IPaymentNotificationRepository notificationRepository,
        IIapPurchaseRepository purchaseRepository,
        IClock clock)
    {
        _notificationRepository = notificationRepository;
        _purchaseRepository = purchaseRepository;
        _clock = clock;
    }

    internal async Task<PaymentNotificationResultDto> ProcessAsync(
        PaymentNotificationVerificationResult verification,
        CancellationToken cancellationToken)
    {
        var existing = await _notificationRepository.GetByPlatformAndNotificationIdAsync(
            verification.Platform,
            verification.NotificationId,
            cancellationToken);
        if (existing is not null)
        {
            return ToResult(existing, purchase: null, isReplay: true);
        }

        var notification = PaymentNotification.Receive(
            verification.Platform,
            verification.Environment,
            verification.NotificationId,
            verification.NotificationType,
            verification.PayloadJson,
            _clock.UtcNow);
        await _notificationRepository.AddAsync(notification, cancellationToken);

        if (!verification.IsVerified)
        {
            notification.MarkFailed(verification.FailureReason ?? "Payment notification verification failed.");
            return ToResult(notification, purchase: null, isReplay: false);
        }

        var purchase = await FindPurchaseAsync(verification, cancellationToken);
        if (purchase is null)
        {
            notification.MarkFailed("Matching IAP purchase was not found.");
            return ToResult(notification, purchase: null, isReplay: false);
        }

        ApplyPurchaseStatus(purchase, verification);
        notification.MarkProcessed(_clock.UtcNow);

        return ToResult(notification, purchase, isReplay: false);
    }

    private async Task<IapPurchase?> FindPurchaseAsync(
        PaymentNotificationVerificationResult verification,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(verification.StoreTransactionId))
        {
            return await _purchaseRepository.GetByStoreTransactionIdAsync(
                verification.Platform,
                StoreTransactionId.Of(verification.StoreTransactionId),
                cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(verification.PurchaseToken))
        {
            return await _purchaseRepository.GetByPurchaseTokenAsync(
                verification.Platform,
                PurchaseToken.Of(verification.PurchaseToken),
                cancellationToken);
        }

        return null;
    }

    private static void ApplyPurchaseStatus(
        IapPurchase purchase,
        PaymentNotificationVerificationResult verification)
    {
        var reason = verification.Reason ?? verification.NotificationType;
        switch (verification.PurchaseStatus)
        {
            case PaymentNotificationPurchaseStatus.Refunded:
                purchase.MarkRefunded(reason);
                break;
            case PaymentNotificationPurchaseStatus.Revoked:
                purchase.MarkRevoked(reason);
                break;
            case PaymentNotificationPurchaseStatus.Failed:
                purchase.MarkFailed(reason);
                break;
            case PaymentNotificationPurchaseStatus.None:
            default:
                break;
        }
    }

    private static PaymentNotificationResultDto ToResult(
        PaymentNotification notification,
        IapPurchase? purchase,
        bool isReplay) =>
        new(
            notification.Id.Value,
            notification.Platform.ToString(),
            notification.NotificationType,
            notification.Status.ToString(),
            purchase?.Id.Value,
            purchase?.Status.ToString(),
            isReplay,
            notification.FailureReason);
}
