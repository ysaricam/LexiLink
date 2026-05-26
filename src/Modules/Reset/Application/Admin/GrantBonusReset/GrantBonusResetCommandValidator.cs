using FluentValidation;

namespace LexiLink.Modules.Reset.Application.Admin.GrantBonusReset;

internal sealed class GrantBonusResetCommandValidator : AbstractValidator<GrantBonusResetCommand>
{
    public GrantBonusResetCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
