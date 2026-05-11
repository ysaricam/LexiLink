using FluentValidation;
using LexiLink.Modules.Players.Domain.Players.Rules;

namespace LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;

internal class RegisterGuestPlayerCommandValidator : AbstractValidator<RegisterGuestPlayerCommand>
{
    public RegisterGuestPlayerCommandValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty();

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MinimumLength(DisplayNameMustMeetMinimumLengthRule.MinLength)
            .MaximumLength(DisplayNameMustNotExceedMaxLengthRule.MaxLength);

        RuleFor(x => x.Locale)
            .NotEmpty();
    }
}
