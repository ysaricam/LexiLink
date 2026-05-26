using LexiLink.Modules.Undo.Application.Configuration.Commands;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;

namespace LexiLink.Modules.Undo.Application.PlayerUndoInventories.EnsurePlayerUndoInventoryExists;

/// <summary>
/// Idempotent initialization of a player's undo inventory. Invoked by
/// the PlayerRegisteredIntegrationEvent consumer and any path that
/// needs to ensure the row exists before granting or consuming.
/// </summary>
internal class EnsurePlayerUndoInventoryExistsCommandHandler
    : ICommandHandler<EnsurePlayerUndoInventoryExistsCommand>
{
    private readonly IPlayerUndoInventoryRepository _repository;
    private readonly IUndoConfigurationService _undoConfiguration;

    internal EnsurePlayerUndoInventoryExistsCommandHandler(
        IPlayerUndoInventoryRepository repository,
        IUndoConfigurationService undoConfiguration)
    {
        _repository = repository;
        _undoConfiguration = undoConfiguration;
    }

    public async Task Handle(EnsurePlayerUndoInventoryExistsCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(
            new PlayerUndoInventoryId(request.PlayerId),
            cancellationToken);

        if (existing is not null)
        {
            return;
        }

        var inventory = PlayerUndoInventory.InitializeFor(
            request.PlayerId,
            _undoConfiguration.InitialBalance);

        await _repository.AddAsync(inventory, cancellationToken);
    }
}
