using LexiLink.Modules.Undo.Application.Contracts;

namespace LexiLink.Modules.Undo.Application.PlayerUndoInventories.GetPlayerUndo;

public class GetPlayerUndoQuery : QueryBase<PlayerUndoSnapshotDto>
{
    public Guid PlayerId { get; }
    public bool UseGameplayPresentation { get; }

    public GetPlayerUndoQuery(Guid playerId, bool useGameplayPresentation = true)
    {
        PlayerId = playerId;
        UseGameplayPresentation = useGameplayPresentation;
    }
}
