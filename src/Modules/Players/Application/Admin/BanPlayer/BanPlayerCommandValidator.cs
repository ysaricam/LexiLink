using FluentValidation;

namespace LexiLink.Modules.Players.Application.Admin.BanPlayer;

internal sealed class BanPlayerCommandValidator : AbstractValidator<BanPlayerCommand>
{
    public const int MaxReasonLength = 500;

    public BanPlayerCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(MaxReasonLength);
    }
}
