namespace LexiLink.Modules.Quests.Application.Configuration.CrossModule;

public interface IQuestEnergyRewardGuard
{
    Task EnsureEnergyRewardCanBeAcceptedAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default);
}
