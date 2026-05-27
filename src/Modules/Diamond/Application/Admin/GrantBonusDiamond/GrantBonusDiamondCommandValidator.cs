using FluentValidation;

namespace LexiLink.Modules.Diamond.Application.Admin.GrantBonusDiamond;

internal sealed class GrantBonusDiamondCommandValidator : AbstractValidator<GrantBonusDiamondCommand>
{
    public GrantBonusDiamondCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
