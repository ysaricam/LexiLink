using LexiLink.Common.Application.Time;
using LexiLink.Modules.Energy.Application.Configuration.Commands;
using LexiLink.Modules.Energy.Domain.PlayerEnergies;

namespace LexiLink.Modules.Energy.Application.PlayerEnergies.EnsurePlayerEnergyExists;

internal class EnsurePlayerEnergyExistsCommandHandler : ICommandHandler<EnsurePlayerEnergyExistsCommand>
{
    private readonly IPlayerEnergyRepository _playerEnergyRepository;
    private readonly IEnergyConfigurationService _energyConfiguration;
    private readonly IClock _clock;

    internal EnsurePlayerEnergyExistsCommandHandler(
        IPlayerEnergyRepository playerEnergyRepository,
        IEnergyConfigurationService energyConfiguration,
        IClock clock)
    {
        _playerEnergyRepository = playerEnergyRepository;
        _energyConfiguration = energyConfiguration;
        _clock = clock;
    }

    public async Task Handle(EnsurePlayerEnergyExistsCommand request, CancellationToken cancellationToken)
    {
        var existing = await _playerEnergyRepository.GetByIdAsync(
            new PlayerEnergyId(request.PlayerId),
            cancellationToken);

        if (existing is not null)
        {
            return;
        }

        var playerEnergy = PlayerEnergy.InitializeFor(
            request.PlayerId,
            _energyConfiguration.MaximumAmount,
            _energyConfiguration.RechargeIntervalSeconds,
            _clock.UtcNow);

        await _playerEnergyRepository.AddAsync(playerEnergy, cancellationToken);
    }
}
