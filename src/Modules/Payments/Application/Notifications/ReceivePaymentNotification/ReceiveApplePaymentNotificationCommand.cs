using LexiLink.Modules.Payments.Application.Contracts;

namespace LexiLink.Modules.Payments.Application.Notifications.ReceivePaymentNotification;

public sealed class ReceiveApplePaymentNotificationCommand : CommandBase<PaymentNotificationResultDto>
{
    public string SignedPayload { get; }

    public ReceiveApplePaymentNotificationCommand(string signedPayload)
    {
        SignedPayload = signedPayload;
    }
}
