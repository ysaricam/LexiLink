using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Hint.Application.Contracts;
using LexiLink.Modules.Hint.Application.PlayerHintInventories.EnsurePlayerHintInventoryExists;
using LexiLink.Modules.Hint.Application.PlayerHintInventories.GrantHint;
using LexiLink.Modules.Quests.IntegrationEvents;

namespace LexiLink.Modules.Hint.Application.PlayerHintInventories.ProcessIntegrationEvents;

/// <summary>
/// Hint consumes <see cref="QuestClaimedIntegrationEvent"/> and grants
/// the player <c>HintReward</c> charges. Mirrors Energy's consumer.
/// Each resource handler no-ops when its share of the reward is zero.
/// </summary>
internal class QuestClaimedIntegrationEventHandler :
    IIntegrationEventHandler<QuestClaimedIntegrationEvent>
{
    private readonly IHintModule _hintModule;

    internal QuestClaimedIntegrationEventHandler(IHintModule hintModule)
    {
        _hintModule = hintModule;
    }

    public async Task Handle(QuestClaimedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        if (integrationEvent.HintReward <= 0)
        {
            return;
        }

        // Defensively ensure the hint inventory exists. Idempotent —
        // under normal flow PlayerRegisteredIntegrationEvent already
        // initialized it.
        await _hintModule.ExecuteCommandAsync(
            new EnsurePlayerHintInventoryExistsCommand(integrationEvent.PlayerId),
            cancellationToken);

        await _hintModule.ExecuteCommandAsync(
            new GrantHintCommand(integrationEvent.PlayerId, integrationEvent.HintReward),
            cancellationToken);
    }
}
