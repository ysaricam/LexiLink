using FluentValidation;

namespace LexiLink.Modules.Reset.Application.PlayerResetInventories.EnsurePlayerResetInventoryExists;

internal class EnsurePlayerResetInventoryExistsCommandValidator
    : AbstractValidator<EnsurePlayerResetInventoryExistsCommand>
{
    public EnsurePlayerResetInventoryExistsCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
    }
}
