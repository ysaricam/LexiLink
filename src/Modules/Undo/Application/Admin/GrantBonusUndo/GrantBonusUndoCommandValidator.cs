using FluentValidation;

namespace LexiLink.Modules.Undo.Application.Admin.GrantBonusUndo;

internal sealed class GrantBonusUndoCommandValidator : AbstractValidator<GrantBonusUndoCommand>
{
    public GrantBonusUndoCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
