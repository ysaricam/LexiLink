using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Energy.Application.Contracts;
using LexiLink.Modules.Energy.Application.PlayerEnergies.EnsurePlayerEnergyExists;
using LexiLink.Modules.Energy.Application.PlayerEnergies.GrantEnergy;
using LexiLink.Modules.Quests.IntegrationEvents;

namespace LexiLink.Modules.Energy.Application.PlayerEnergies.ProcessIntegrationEvents;

internal class QuestClaimedIntegrationEventHandler :
    IIntegrationEventHandler<QuestClaimedIntegrationEvent>
{
    private readonly IEnergyModule _energyModule;

    internal QuestClaimedIntegrationEventHandler(IEnergyModule energyModule)
    {
        _energyModule = energyModule;
    }

    public async Task Handle(QuestClaimedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        // Defensively ensure the energy aggregate exists. Under normal flow it
        // was already initialized via PlayerRegisteredIntegrationEvent, but quest
        // claims can race with that init under retries; EnsurePlayerEnergyExists
        // is idempotent.
        await _energyModule.ExecuteCommandAsync(
            new EnsurePlayerEnergyExistsCommand(integrationEvent.PlayerId),
            cancellationToken);

        await _energyModule.ExecuteCommandAsync(
            new GrantEnergyCommand(integrationEvent.PlayerId, integrationEvent.Reward),
            cancellationToken);
    }
}
