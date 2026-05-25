using FluentValidation;

namespace LexiLink.Modules.Hint.Application.Admin.SetPlayerHint;

internal sealed class SetPlayerHintCommandValidator : AbstractValidator<SetPlayerHintCommand>
{
    public SetPlayerHintCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Balance).GreaterThanOrEqualTo(0);
    }
}
