namespace LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories;

public interface IDiamondConfigurationService
{
    /// <summary>
    /// Balance assigned to a freshly initialized PlayerDiamondInventory.
    /// Operator-tunable via the <c>Diamond:InitialBalance</c> config key;
    /// defaults to 0 so first-time players earn diamonds exclusively
    /// through quest rewards.
    /// </summary>
    int InitialBalance { get; }
}
