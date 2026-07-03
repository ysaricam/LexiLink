using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Energy.Application.Configuration.Commands;
using LexiLink.Modules.Energy.Domain.PlayerEnergies;

namespace LexiLink.Modules.Energy.Application.PlayerEnergies.GrantEnergy;

internal sealed class GrantCappedEnergyCommandHandler : ICommandHandler<GrantCappedEnergyCommand, int>
{
    private readonly IPlayerEnergyRepository _playerEnergyRepository;
    private readonly IClock _clock;

    internal GrantCappedEnergyCommandHandler(
        IPlayerEnergyRepository playerEnergyRepository,
        IClock clock)
    {
        _playerEnergyRepository = playerEnergyRepository;
        _clock = clock;
    }

    public async Task<int> Handle(GrantCappedEnergyCommand request, CancellationToken cancellationToken)
    {
        var playerEnergy = await _playerEnergyRepository.GetByIdAsync(
            new PlayerEnergyId(request.PlayerId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerEnergy), request.PlayerId);

        return playerEnergy.GrantBonusCapped(request.Amount, _clock.UtcNow);
    }
}
