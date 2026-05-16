using LexiLink.Common.Application.Time;
using LexiLink.Modules.Players.Application.Configuration.Commands;
using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;

internal class RegisterGuestPlayerCommandHandler : ICommandHandler<RegisterGuestPlayerCommand, Guid>
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IDiscriminatorGenerator _discriminatorGenerator;
    private readonly IClock _clock;

    internal RegisterGuestPlayerCommandHandler(
        IPlayerRepository playerRepository,
        IDiscriminatorGenerator discriminatorGenerator,
        IClock clock)
    {
        _playerRepository = playerRepository;
        _discriminatorGenerator = discriminatorGenerator;
        _clock = clock;
    }

    public async Task<Guid> Handle(RegisterGuestPlayerCommand request, CancellationToken cancellationToken)
    {
        var existingGuest = await _playerRepository.GetByAuthProviderAsync(
            AuthProvider.Guest,
            request.DeviceId,
            cancellationToken);

        if (existingGuest is not null)
        {
            return existingGuest.Id.Value;
        }

        var discriminator = await _discriminatorGenerator.GenerateForAsync(request.DisplayName, cancellationToken);

        var player = Player.RegisterGuest(
            deviceId: request.DeviceId,
            displayName: request.DisplayName,
            discriminator: discriminator,
            locale: request.Locale,
            registeredAt: _clock.UtcNow);

        await _playerRepository.AddAsync(player, cancellationToken);

        return player.Id.Value;
    }
}
