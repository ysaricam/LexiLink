using FluentValidation;

namespace LexiLink.Modules.Games.Application.Games.Undo;

internal class UndoCommandValidator : AbstractValidator<UndoCommand>
{
    public UndoCommandValidator()
    {
        RuleFor(x => x.GameId).NotEmpty();
    }
}
