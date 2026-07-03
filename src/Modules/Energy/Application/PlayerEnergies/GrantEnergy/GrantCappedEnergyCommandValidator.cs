using FluentValidation;

namespace LexiLink.Modules.Energy.Application.PlayerEnergies.GrantEnergy;

internal sealed class GrantCappedEnergyCommandValidator : AbstractValidator<GrantCappedEnergyCommand>
{
    public GrantCappedEnergyCommandValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}
