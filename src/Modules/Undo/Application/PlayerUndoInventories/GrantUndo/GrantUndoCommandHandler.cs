using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Undo.Application.Configuration.Commands;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;

namespace LexiLink.Modules.Undo.Application.PlayerUndoInventories.GrantUndo;

internal class GrantUndoCommandHandler : ICommandHandler<GrantUndoCommand>
{
    private readonly IPlayerUndoInventoryRepository _repository;
    private readonly IClock _clock;

    internal GrantUndoCommandHandler(
        IPlayerUndoInventoryRepository repository,
        IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task Handle(GrantUndoCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _repository.GetByIdAsync(
            new PlayerUndoInventoryId(request.PlayerId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerUndoInventory), request.PlayerId);

        inventory.GrantBonus(request.Amount, _clock.UtcNow);
    }
}
