using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Energy.Application.Configuration.Commands;
using LexiLink.Modules.Energy.Domain.PlayerEnergies;

namespace LexiLink.Modules.Energy.Application.Admin.GrantBonusEnergy;

internal sealed class GrantBonusEnergyCommandHandler : ICommandHandler<GrantBonusEnergyCommand>
{
    private readonly IPlayerEnergyRepository _playerEnergyRepository;
    private readonly IClock _clock;

    internal GrantBonusEnergyCommandHandler(
        IPlayerEnergyRepository playerEnergyRepository,
        IClock clock)
    {
        _playerEnergyRepository = playerEnergyRepository;
        _clock = clock;
    }

    public async Task Handle(GrantBonusEnergyCommand request, CancellationToken cancellationToken)
    {
        var playerEnergy = await _playerEnergyRepository.GetByIdAsync(
            new PlayerEnergyId(request.PlayerId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerEnergy), request.PlayerId);

        playerEnergy.GrantBonusCapped(request.Amount, _clock.UtcNow);
    }
}
