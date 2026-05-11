using FluentValidation;

namespace LexiLink.Modules.Games.Application.Games.AbandonGame;

internal class AbandonGameCommandValidator : AbstractValidator<AbandonGameCommand>
{
    public AbandonGameCommandValidator()
    {
        RuleFor(x => x.GameId).NotEmpty();
    }
}
