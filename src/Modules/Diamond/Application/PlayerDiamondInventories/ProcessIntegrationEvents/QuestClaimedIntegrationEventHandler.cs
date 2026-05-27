using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Diamond.Application.Contracts;
using LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.EnsurePlayerDiamondInventoryExists;
using LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.GrantDiamond;
using LexiLink.Modules.Quests.IntegrationEvents;

namespace LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.ProcessIntegrationEvents;

internal class QuestClaimedIntegrationEventHandler :
    IIntegrationEventHandler<QuestClaimedIntegrationEvent>
{
    private readonly IDiamondModule _diamondModule;

    internal QuestClaimedIntegrationEventHandler(IDiamondModule diamondModule)
    {
        _diamondModule = diamondModule;
    }

    public async Task Handle(QuestClaimedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        if (integrationEvent.DiamondReward <= 0)
        {
            return;
        }

        await _diamondModule.ExecuteCommandAsync(
            new EnsurePlayerDiamondInventoryExistsCommand(integrationEvent.PlayerId),
            cancellationToken);

        await _diamondModule.ExecuteCommandAsync(
            new GrantDiamondCommand(integrationEvent.PlayerId, integrationEvent.DiamondReward),
            cancellationToken);
    }
}
