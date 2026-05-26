using FluentValidation;

namespace LexiLink.Modules.Reset.Application.PlayerResetInventories.GrantReset;

internal class GrantResetCommandValidator : AbstractValidator<GrantResetCommand>
{
    public GrantResetCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
