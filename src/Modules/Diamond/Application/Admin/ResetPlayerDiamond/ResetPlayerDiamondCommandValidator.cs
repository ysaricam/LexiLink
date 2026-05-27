using FluentValidation;

namespace LexiLink.Modules.Diamond.Application.Admin.ResetPlayerDiamond;

internal sealed class ResetPlayerDiamondCommandValidator : AbstractValidator<ResetPlayerDiamondCommand>
{
    public ResetPlayerDiamondCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
    }
}
