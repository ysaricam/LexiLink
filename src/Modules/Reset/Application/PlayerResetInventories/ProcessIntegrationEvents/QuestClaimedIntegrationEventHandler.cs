using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Quests.IntegrationEvents;
using LexiLink.Modules.Reset.Application.Contracts;
using LexiLink.Modules.Reset.Application.PlayerResetInventories.EnsurePlayerResetInventoryExists;
using LexiLink.Modules.Reset.Application.PlayerResetInventories.GrantReset;

namespace LexiLink.Modules.Reset.Application.PlayerResetInventories.ProcessIntegrationEvents;

internal class QuestClaimedIntegrationEventHandler :
    IIntegrationEventHandler<QuestClaimedIntegrationEvent>
{
    private readonly IResetModule _resetModule;

    internal QuestClaimedIntegrationEventHandler(IResetModule resetModule)
    {
        _resetModule = resetModule;
    }

    public async Task Handle(QuestClaimedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        if (integrationEvent.ResetReward <= 0)
        {
            return;
        }

        await _resetModule.ExecuteCommandAsync(
            new EnsurePlayerResetInventoryExistsCommand(integrationEvent.PlayerId),
            cancellationToken);

        await _resetModule.ExecuteCommandAsync(
            new GrantResetCommand(integrationEvent.PlayerId, integrationEvent.ResetReward),
            cancellationToken);
    }
}
