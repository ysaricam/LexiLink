using FluentValidation;

namespace LexiLink.Modules.Payments.Application.Notifications.ReceivePaymentNotification;

internal sealed class ReceiveApplePaymentNotificationCommandValidator
    : AbstractValidator<ReceiveApplePaymentNotificationCommand>
{
    public ReceiveApplePaymentNotificationCommandValidator()
    {
        RuleFor(x => x.SignedPayload).NotEmpty();
    }
}
