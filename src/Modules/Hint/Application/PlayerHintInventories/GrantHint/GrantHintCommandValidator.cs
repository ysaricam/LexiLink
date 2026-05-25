using FluentValidation;

namespace LexiLink.Modules.Hint.Application.PlayerHintInventories.GrantHint;

internal class GrantHintCommandValidator : AbstractValidator<GrantHintCommand>
{
    public GrantHintCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
