using FluentValidation;

namespace LexiLink.Modules.Payments.Application.Notifications.ReceivePaymentNotification;

internal sealed class ReceiveGooglePaymentNotificationCommandValidator
    : AbstractValidator<ReceiveGooglePaymentNotificationCommand>
{
    public ReceiveGooglePaymentNotificationCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty().MaximumLength(256);
        RuleFor(x => x.PayloadJson).NotEmpty();
    }
}
