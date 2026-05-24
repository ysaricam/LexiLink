using FluentValidation;

namespace LexiLink.Modules.Hint.Application.PlayerHintInventories.EnsurePlayerHintInventoryExists;

internal class EnsurePlayerHintInventoryExistsCommandValidator
    : AbstractValidator<EnsurePlayerHintInventoryExistsCommand>
{
    public EnsurePlayerHintInventoryExistsCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
    }
}
