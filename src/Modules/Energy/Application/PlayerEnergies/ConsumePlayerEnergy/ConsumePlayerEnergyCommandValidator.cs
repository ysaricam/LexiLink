using FluentValidation;

namespace LexiLink.Modules.Energy.Application.PlayerEnergies.ConsumePlayerEnergy;

internal class ConsumePlayerEnergyCommandValidator : AbstractValidator<ConsumePlayerEnergyCommand>
{
    public ConsumePlayerEnergyCommandValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}
