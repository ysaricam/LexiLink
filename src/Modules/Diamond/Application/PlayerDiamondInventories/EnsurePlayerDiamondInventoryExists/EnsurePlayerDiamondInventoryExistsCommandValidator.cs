using FluentValidation;

namespace LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.EnsurePlayerDiamondInventoryExists;

internal class EnsurePlayerDiamondInventoryExistsCommandValidator
    : AbstractValidator<EnsurePlayerDiamondInventoryExistsCommand>
{
    public EnsurePlayerDiamondInventoryExistsCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
    }
}
