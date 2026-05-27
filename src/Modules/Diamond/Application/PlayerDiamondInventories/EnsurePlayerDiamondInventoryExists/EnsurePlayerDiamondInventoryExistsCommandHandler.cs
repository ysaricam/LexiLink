using LexiLink.Modules.Diamond.Application.Configuration.Commands;
using LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories;

namespace LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.EnsurePlayerDiamondInventoryExists;

/// <summary>
/// Idempotent initialization of a player's Diamond inventory. Invoked by
/// PlayerRegisteredIntegrationEvent and any future path that needs the row
/// before granting or consuming currency.
/// </summary>
internal class EnsurePlayerDiamondInventoryExistsCommandHandler
    : ICommandHandler<EnsurePlayerDiamondInventoryExistsCommand>
{
    private readonly IPlayerDiamondInventoryRepository _repository;
    private readonly IDiamondConfigurationService _diamondConfiguration;

    internal EnsurePlayerDiamondInventoryExistsCommandHandler(
        IPlayerDiamondInventoryRepository repository,
        IDiamondConfigurationService diamondConfiguration)
    {
        _repository = repository;
        _diamondConfiguration = diamondConfiguration;
    }

    public async Task Handle(EnsurePlayerDiamondInventoryExistsCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(
            new PlayerDiamondInventoryId(request.PlayerId),
            cancellationToken);

        if (existing is not null)
        {
            return;
        }

        var inventory = PlayerDiamondInventory.InitializeFor(
            request.PlayerId,
            _diamondConfiguration.InitialBalance);

        await _repository.AddAsync(inventory, cancellationToken);
    }
}
