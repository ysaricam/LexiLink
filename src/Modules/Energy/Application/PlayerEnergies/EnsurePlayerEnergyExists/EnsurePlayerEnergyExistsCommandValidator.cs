using FluentValidation;

namespace LexiLink.Modules.Energy.Application.PlayerEnergies.EnsurePlayerEnergyExists;

internal class EnsurePlayerEnergyExistsCommandValidator : AbstractValidator<EnsurePlayerEnergyExistsCommand>
{
    public EnsurePlayerEnergyExistsCommandValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty();
    }
}
