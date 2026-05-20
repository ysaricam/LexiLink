using FluentValidation;

namespace LexiLink.Modules.Energy.Application.Admin.SetPlayerEnergy;

internal sealed class SetPlayerEnergyCommandValidator : AbstractValidator<SetPlayerEnergyCommand>
{
    public SetPlayerEnergyCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
    }
}
