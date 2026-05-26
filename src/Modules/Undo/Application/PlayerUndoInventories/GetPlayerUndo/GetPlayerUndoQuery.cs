using LexiLink.Modules.Undo.Application.Contracts;

namespace LexiLink.Modules.Undo.Application.PlayerUndoInventories.GetPlayerUndo;

public class GetPlayerUndoQuery : QueryBase<PlayerUndoSnapshotDto>
{
    public Guid PlayerId { get; }

    public GetPlayerUndoQuery(Guid playerId)
    {
        PlayerId = playerId;
    }
}
