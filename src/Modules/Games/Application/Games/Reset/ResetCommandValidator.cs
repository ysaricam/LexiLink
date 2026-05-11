using FluentValidation;

namespace LexiLink.Modules.Games.Application.Games.Reset;

internal class ResetCommandValidator : AbstractValidator<ResetCommand>
{
    public ResetCommandValidator()
    {
        RuleFor(x => x.GameId).NotEmpty();
    }
}
