using FluentValidation;

namespace LexiLink.Modules.Hint.Application.Admin.GrantBonusHint;

internal sealed class GrantBonusHintCommandValidator : AbstractValidator<GrantBonusHintCommand>
{
    public GrantBonusHintCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
