namespace LexiLink.Modules.Reset.Domain.PlayerResetInventories;

public interface IResetConfigurationService
{
    /// <summary>
    /// Balance assigned to a freshly initialized PlayerResetInventory.
    /// Operator-tunable via the <c>Reset:InitialBalance</c> config key;
    /// defaults to 0 so first-time players earn resets exclusively
    /// through quest rewards.
    /// </summary>
    int InitialBalance { get; }
}
