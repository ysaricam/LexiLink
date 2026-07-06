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

    /// <summary>
    /// When enabled, in-game undo calls do not consume the player's
    /// persistent undo inventory. History/state rules still apply.
    /// </summary>
    bool UnlimitedGameplayUndoEnabled { get; }

    /// <summary>
    /// Positive balance shown to the player-facing undo endpoint while
    /// unlimited gameplay undo is enabled. This keeps existing clients
    /// sending undo requests without changing their UI logic.
    /// </summary>
    int UnlimitedGameplayBalance { get; }
}
