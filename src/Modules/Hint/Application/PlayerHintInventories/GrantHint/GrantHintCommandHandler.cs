using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Hint.Application.Configuration.Commands;
using LexiLink.Modules.Hint.Domain.PlayerHintInventories;

namespace LexiLink.Modules.Hint.Application.PlayerHintInventories.GrantHint;

internal class GrantHintCommandHandler : ICommandHandler<GrantHintCommand>
{
    private readonly IPlayerHintInventoryRepository _repository;
    private readonly IClock _clock;

    internal GrantHintCommandHandler(
        IPlayerHintInventoryRepository repository,
        IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task Handle(GrantHintCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _repository.GetByIdAsync(
            new PlayerHintInventoryId(request.PlayerId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerHintInventory), request.PlayerId);

        inventory.GrantBonus(request.Amount, _clock.UtcNow);
    }
}
