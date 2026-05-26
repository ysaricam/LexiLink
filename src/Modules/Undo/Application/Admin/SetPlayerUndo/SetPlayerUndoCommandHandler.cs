using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Undo.Application.Configuration.Commands;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;

namespace LexiLink.Modules.Undo.Application.Admin.SetPlayerUndo;

internal sealed class SetPlayerUndoCommandHandler : ICommandHandler<SetPlayerUndoCommand>
{
    private readonly IPlayerUndoInventoryRepository _repository;
    private readonly IClock _clock;

    internal SetPlayerUndoCommandHandler(IPlayerUndoInventoryRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task Handle(SetPlayerUndoCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _repository.GetByIdAsync(
            new PlayerUndoInventoryId(request.PlayerId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerUndoInventory), request.PlayerId);

        inventory.AdminSet(request.Balance, _clock.UtcNow);
    }
}
