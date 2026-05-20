using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Energy.Application.Configuration.Commands;
using LexiLink.Modules.Energy.Domain.PlayerEnergies;

namespace LexiLink.Modules.Energy.Application.Admin.ResetPlayerEnergy;

internal sealed class ResetPlayerEnergyCommandHandler : ICommandHandler<ResetPlayerEnergyCommand>
{
    private readonly IPlayerEnergyRepository _repository;
    private readonly IClock _clock;

    internal ResetPlayerEnergyCommandHandler(IPlayerEnergyRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task Handle(ResetPlayerEnergyCommand request, CancellationToken cancellationToken)
    {
        var energy = await _repository.GetByIdAsync(
            new PlayerEnergyId(request.PlayerId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerEnergy), request.PlayerId);

        energy.AdminReset(_clock.UtcNow);
    }
}
