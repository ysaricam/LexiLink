using LexiLink.Modules.Hint.Application.Configuration.Commands;
using LexiLink.Modules.Hint.Domain.PlayerHintInventories;

namespace LexiLink.Modules.Hint.Application.PlayerHintInventories.EnsurePlayerHintInventoryExists;

/// <summary>
/// Idempotent initialization of a player's hint inventory. Invoked by
/// the <c>PlayerRegisteredIntegrationEvent</c> consumer (lazy init,
/// mirroring Energy's pattern) and any other path that needs to make
/// sure the row exists before granting / consuming. No-op when the
/// aggregate is already persisted.
/// </summary>
internal class EnsurePlayerHintInventoryExistsCommandHandler
    : ICommandHandler<EnsurePlayerHintInventoryExistsCommand>
{
    private readonly IPlayerHintInventoryRepository _repository;
    private readonly IHintConfigurationService _hintConfiguration;

    internal EnsurePlayerHintInventoryExistsCommandHandler(
        IPlayerHintInventoryRepository repository,
        IHintConfigurationService hintConfiguration)
    {
        _repository = repository;
        _hintConfiguration = hintConfiguration;
    }

    public async Task Handle(EnsurePlayerHintInventoryExistsCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(
            new PlayerHintInventoryId(request.PlayerId),
            cancellationToken);

        if (existing is not null)
        {
            return;
        }

        var inventory = PlayerHintInventory.InitializeFor(
            request.PlayerId,
            _hintConfiguration.InitialBalance);

        await _repository.AddAsync(inventory, cancellationToken);
    }
}
