using FluentValidation;

namespace LexiLink.Modules.Games.Application.Games.MakeStep;

internal class MakeStepCommandValidator : AbstractValidator<MakeStepCommand>
{
    public MakeStepCommandValidator()
    {
        RuleFor(x => x.GameId).NotEmpty();
        RuleFor(x => x.NextLinkId).NotEmpty();
    }
}
