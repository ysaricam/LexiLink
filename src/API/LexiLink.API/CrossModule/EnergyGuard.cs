using LexiLink.Modules.Energy.Application.Contracts;
using LexiLink.Modules.Energy.Application.PlayerEnergies.ConsumePlayerEnergy;
using LexiLink.Modules.Energy.Domain.PlayerEnergies;
using LexiLink.Modules.Games.Application.Configuration.CrossModule;

namespace LexiLink.API.CrossModule;

// API-host adapter for the cross-module Games → Energy gateway. Lives in the
// composition root so neither Games nor Energy needs a structural reference to
// the other. Translates EnsureCanStartGameAsync into Energy's
// ConsumePlayerEnergyCommand. If energy is insufficient the underlying
// BusinessRuleValidationException propagates to the caller.
internal class EnergyGuard : IEnergyGuard
{
    private readonly IEnergyModule _energyModule;
    private readonly IEnergyConfigurationService _energyConfiguration;

    public EnergyGuard(IEnergyModule energyModule, IEnergyConfigurationService energyConfiguration)
    {
        _energyModule = energyModule;
        _energyConfiguration = energyConfiguration;
    }

    public Task EnsureCanStartGameAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        return _energyModule.ExecuteCommandAsync(
            new ConsumePlayerEnergyCommand(playerId, _energyConfiguration.GameStartCost),
            cancellationToken);
    }
}
