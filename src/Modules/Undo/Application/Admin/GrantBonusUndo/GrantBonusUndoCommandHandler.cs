using MediatR;
using LexiLink.Modules.Undo.Application.Configuration.Commands;
using LexiLink.Modules.Undo.Application.PlayerUndoInventories.GrantUndo;

namespace LexiLink.Modules.Undo.Application.Admin.GrantBonusUndo;

internal sealed class GrantBonusUndoCommandHandler : ICommandHandler<GrantBonusUndoCommand>
{
    private readonly ISender _sender;

    internal GrantBonusUndoCommandHandler(ISender sender)
    {
        _sender = sender;
    }

    public Task Handle(GrantBonusUndoCommand request, CancellationToken cancellationToken) =>
        // Wraps the internal GrantUndoCommand so the bonus path
        // (over-cap allowed — undo inventory has no cap) stays in
        // one place.
        _sender.Send(new GrantUndoCommand(request.PlayerId, request.Amount), cancellationToken);
}
