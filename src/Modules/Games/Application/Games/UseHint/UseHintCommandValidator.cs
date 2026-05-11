using FluentValidation;

namespace LexiLink.Modules.Games.Application.Games.UseHint;

internal class UseHintCommandValidator : AbstractValidator<UseHintCommand>
{
    public UseHintCommandValidator()
    {
        RuleFor(x => x.GameId).NotEmpty();
    }
}
