using FluentValidation;

namespace LexiLink.Modules.Games.Application.Games.StartGame;

internal class StartGameCommandValidator : AbstractValidator<StartGameCommand>
{
    public StartGameCommandValidator()
    {
        RuleFor(x => x.GameId).NotEmpty();
    }
}
