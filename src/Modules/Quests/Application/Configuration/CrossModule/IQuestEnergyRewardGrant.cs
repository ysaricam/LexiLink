namespace LexiLink.Modules.Quests.Application.Configuration.CrossModule;

public interface IQuestEnergyRewardGrant
{
    Task<int> GrantEnergyRewardAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default);
}
