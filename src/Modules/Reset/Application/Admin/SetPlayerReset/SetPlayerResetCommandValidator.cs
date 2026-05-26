using FluentValidation;

namespace LexiLink.Modules.Reset.Application.Admin.SetPlayerReset;

internal sealed class SetPlayerResetCommandValidator : AbstractValidator<SetPlayerResetCommand>
{
    public SetPlayerResetCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Balance).GreaterThanOrEqualTo(0);
    }
}
