using FluentValidation;

namespace LexiLink.Modules.Reset.Application.PlayerResetInventories.ConsumePlayerReset;

internal class ConsumePlayerResetCommandValidator : AbstractValidator<ConsumePlayerResetCommand>
{
    public ConsumePlayerResetCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
