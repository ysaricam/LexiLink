using LexiLink.Modules.Payments.Application.Contracts;

namespace LexiLink.Modules.Payments.Application.Notifications.ReceivePaymentNotification;

public sealed class ReceiveGooglePaymentNotificationCommand : CommandBase<PaymentNotificationResultDto>
{
    public string MessageId { get; }
    public string PayloadJson { get; }
    public string? AuthorizationToken { get; }

    public ReceiveGooglePaymentNotificationCommand(
        string messageId,
        string payloadJson,
        string? authorizationToken)
    {
        MessageId = messageId;
        PayloadJson = payloadJson;
        AuthorizationToken = authorizationToken;
    }
}
