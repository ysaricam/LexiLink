namespace LexiLink.Modules.Undo.Application.PlayerUndoInventories.GetPlayerUndo;

public record PlayerUndoSnapshotDto(
    Guid PlayerId,
    int Balance);
