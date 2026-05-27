using FluentValidation;

namespace LexiLink.Modules.Diamond.Application.Admin.SetPlayerDiamond;

internal sealed class SetPlayerDiamondCommandValidator : AbstractValidator<SetPlayerDiamondCommand>
{
    public SetPlayerDiamondCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Balance).GreaterThanOrEqualTo(0);
    }
}
