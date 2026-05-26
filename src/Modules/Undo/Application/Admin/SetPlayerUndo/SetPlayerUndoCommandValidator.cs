using FluentValidation;

namespace LexiLink.Modules.Undo.Application.Admin.SetPlayerUndo;

internal sealed class SetPlayerUndoCommandValidator : AbstractValidator<SetPlayerUndoCommand>
{
    public SetPlayerUndoCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Balance).GreaterThanOrEqualTo(0);
    }
}
