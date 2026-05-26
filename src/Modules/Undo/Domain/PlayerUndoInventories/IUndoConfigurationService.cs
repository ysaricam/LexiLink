namespace LexiLink.Modules.Undo.Domain.PlayerUndoInventories;

public interface IUndoConfigurationService
{
    /// <summary>
    /// Balance assigned to a freshly initialized PlayerUndoInventory.
    /// Operator-tunable via the <c>Undo:InitialBalance</c> config key;
    /// defaults to 0 so first-time players earn undos exclusively
    /// through quest rewards.
    /// </summary>
    int InitialBalance { get; }
}
