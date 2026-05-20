using FluentValidation;

namespace LexiLink.Modules.Energy.Application.Admin.GrantBonusEnergy;

internal sealed class GrantBonusEnergyCommandValidator : AbstractValidator<GrantBonusEnergyCommand>
{
    public GrantBonusEnergyCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
