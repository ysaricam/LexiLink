using FluentValidation;

namespace LexiLink.Modules.Energy.Application.PlayerEnergies.GrantEnergy;

internal class GrantEnergyCommandValidator : AbstractValidator<GrantEnergyCommand>
{
    public GrantEnergyCommandValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}
