using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;

namespace LexiLink.Modules.Undo.Application.PlayerUndoInventories.GetPlayerUndo;

internal static class PlayerUndoSnapshotPresenter
{
    internal static PlayerUndoSnapshotDto? PresentOrCreateForGameplay(
        PlayerUndoSnapshotDto? snapshot,
        Guid playerId,
        IUndoConfigurationService undoConfiguration,
        bool useGameplayPresentation)
    {
        if (snapshot is null)
        {
            return useGameplayPresentation && undoConfiguration.UnlimitedGameplayUndoEnabled
                ? new PlayerUndoSnapshotDto(playerId, undoConfiguration.UnlimitedGameplayBalance)
                : null;
        }

        return ApplyGameplayPresentation(
            snapshot,
            undoConfiguration,
            useGameplayPresentation);
    }

    internal static PlayerUndoSnapshotDto ApplyGameplayPresentation(
        PlayerUndoSnapshotDto snapshot,
        IUndoConfigurationService undoConfiguration,
        bool useGameplayPresentation)
    {
        if (!useGameplayPresentation || !undoConfiguration.UnlimitedGameplayUndoEnabled)
        {
            return snapshot;
        }

        return snapshot with { Balance = undoConfiguration.UnlimitedGameplayBalance };
    }
}
