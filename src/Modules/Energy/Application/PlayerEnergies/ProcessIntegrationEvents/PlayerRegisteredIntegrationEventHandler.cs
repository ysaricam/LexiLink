using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Energy.Application.Contracts;
using LexiLink.Modules.Energy.Application.PlayerEnergies.EnsurePlayerEnergyExists;
using LexiLink.Modules.Players.IntegrationEvents;

namespace LexiLink.Modules.Energy.Application.PlayerEnergies.ProcessIntegrationEvents;

internal class PlayerRegisteredIntegrationEventHandler :
    IIntegrationEventHandler<PlayerRegisteredIntegrationEvent>
{
    private readonly IEnergyModule _energyModule;

    internal PlayerRegisteredIntegrationEventHandler(IEnergyModule energyModule)
    {
        _energyModule = energyModule;
    }

    public Task Handle(PlayerRegisteredIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        _energyModule.ExecuteCommandAsync(
            new EnsurePlayerEnergyExistsCommand(integrationEvent.PlayerId),
            cancellationToken);
}
