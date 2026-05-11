using FluentValidation;

namespace LexiLink.Modules.Games.Application.Games.CreateGame;

internal class CreateGameCommandValidator : AbstractValidator<CreateGameCommand>
{
    public CreateGameCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Difficulty).IsInEnum();
    }
}
