using FluentValidation;

namespace LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.ConsumePlayerDiamond;

internal class ConsumePlayerDiamondCommandValidator : AbstractValidator<ConsumePlayerDiamondCommand>
{
    public ConsumePlayerDiamondCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
