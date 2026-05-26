using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Undo.Application.Configuration.Commands;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;

namespace LexiLink.Modules.Undo.Application.Admin.ResetPlayerUndo;

internal sealed class ResetPlayerUndoCommandHandler : ICommandHandler<ResetPlayerUndoCommand>
{
    private readonly IPlayerUndoInventoryRepository _repository;
    private readonly IClock _clock;

    internal ResetPlayerUndoCommandHandler(IPlayerUndoInventoryRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task Handle(ResetPlayerUndoCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _repository.GetByIdAsync(
            new PlayerUndoInventoryId(request.PlayerId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerUndoInventory), request.PlayerId);

        inventory.AdminReset(_clock.UtcNow);
    }
}
