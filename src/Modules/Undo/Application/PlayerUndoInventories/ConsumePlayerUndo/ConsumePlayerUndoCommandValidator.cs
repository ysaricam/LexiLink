using FluentValidation;

namespace LexiLink.Modules.Undo.Application.PlayerUndoInventories.ConsumePlayerUndo;

internal class ConsumePlayerUndoCommandValidator : AbstractValidator<ConsumePlayerUndoCommand>
{
    public ConsumePlayerUndoCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
