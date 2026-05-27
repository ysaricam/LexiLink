using FluentValidation;

namespace LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.GrantDiamond;

internal class GrantDiamondCommandValidator : AbstractValidator<GrantDiamondCommand>
{
    public GrantDiamondCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
