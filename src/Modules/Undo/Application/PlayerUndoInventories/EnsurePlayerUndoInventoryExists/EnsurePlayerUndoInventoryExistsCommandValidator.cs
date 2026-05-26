using FluentValidation;

namespace LexiLink.Modules.Undo.Application.PlayerUndoInventories.EnsurePlayerUndoInventoryExists;

internal class EnsurePlayerUndoInventoryExistsCommandValidator
    : AbstractValidator<EnsurePlayerUndoInventoryExistsCommand>
{
    public EnsurePlayerUndoInventoryExistsCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
    }
}
