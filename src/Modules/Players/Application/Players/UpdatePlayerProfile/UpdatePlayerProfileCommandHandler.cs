using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Players.Application.Configuration.Commands;
using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.Modules.Players.Application.Players.UpdatePlayerProfile;

internal class UpdatePlayerProfileCommandHandler : ICommandHandler<UpdatePlayerProfileCommand>
{
    private readonly IPlayerRepository _playerRepository;

    internal UpdatePlayerProfileCommandHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public async Task Handle(UpdatePlayerProfileCommand request, CancellationToken cancellationToken)
    {
        var player = await _playerRepository.GetByIdAsync(new PlayerId(request.PlayerId), cancellationToken)
            ?? throw new NotFoundException(nameof(Player), request.PlayerId);

        player.UpdateProfile(avatarUrl: request.AvatarUrl, locale: request.Locale);
    }
}
