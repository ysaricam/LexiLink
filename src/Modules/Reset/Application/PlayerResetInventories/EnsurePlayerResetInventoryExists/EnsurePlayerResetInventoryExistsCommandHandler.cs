using LexiLink.Modules.Reset.Application.Configuration.Commands;
using LexiLink.Modules.Reset.Domain.PlayerResetInventories;

namespace LexiLink.Modules.Reset.Application.PlayerResetInventories.EnsurePlayerResetInventoryExists;

/// <summary>
/// Idempotent initialization of a player's reset inventory. Invoked by
/// the PlayerRegisteredIntegrationEvent consumer and any path that
/// needs to ensure the row exists before granting or consuming.
/// </summary>
internal class EnsurePlayerResetInventoryExistsCommandHandler
    : ICommandHandler<EnsurePlayerResetInventoryExistsCommand>
{
    private readonly IPlayerResetInventoryRepository _repository;
    private readonly IResetConfigurationService _resetConfiguration;

    internal EnsurePlayerResetInventoryExistsCommandHandler(
        IPlayerResetInventoryRepository repository,
        IResetConfigurationService resetConfiguration)
    {
        _repository = repository;
        _resetConfiguration = resetConfiguration;
    }

    public async Task Handle(EnsurePlayerResetInventoryExistsCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(
            new PlayerResetInventoryId(request.PlayerId),
            cancellationToken);

        if (existing is not null)
        {
            return;
        }

        var inventory = PlayerResetInventory.InitializeFor(
            request.PlayerId,
            _resetConfiguration.InitialBalance);

        await _repository.AddAsync(inventory, cancellationToken);
    }
}
