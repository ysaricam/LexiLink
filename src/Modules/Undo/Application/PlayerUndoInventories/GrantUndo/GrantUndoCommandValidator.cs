using FluentValidation;

namespace LexiLink.Modules.Undo.Application.PlayerUndoInventories.GrantUndo;

internal class GrantUndoCommandValidator : AbstractValidator<GrantUndoCommand>
{
    public GrantUndoCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
