namespace LexiLink.Modules.Hint.Domain.PlayerHintInventories;

public interface IHintConfigurationService
{
    /// <summary>
    /// Balance assigned to a freshly initialized PlayerHintInventory.
    /// Operator-tunable via the <c>Hint:InitialBalance</c> config key;
    /// defaults to 0 so first-time players earn hints exclusively
    /// through quest rewards.
    /// </summary>
    int InitialBalance { get; }
}
