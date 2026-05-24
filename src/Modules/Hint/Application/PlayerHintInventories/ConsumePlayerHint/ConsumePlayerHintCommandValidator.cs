using FluentValidation;

namespace LexiLink.Modules.Hint.Application.PlayerHintInventories.ConsumePlayerHint;

internal class ConsumePlayerHintCommandValidator : AbstractValidator<ConsumePlayerHintCommand>
{
    public ConsumePlayerHintCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
