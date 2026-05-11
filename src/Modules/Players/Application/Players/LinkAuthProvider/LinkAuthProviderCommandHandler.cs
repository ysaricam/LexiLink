using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Players.Application.Configuration.Commands;
using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.Modules.Players.Application.Players.LinkAuthProvider;

internal class LinkAuthProviderCommandHandler : ICommandHandler<LinkAuthProviderCommand>
{
    private readonly IPlayerRepository _playerRepository;

    internal LinkAuthProviderCommandHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public async Task Handle(LinkAuthProviderCommand request, CancellationToken cancellationToken)
    {
        var player = await _playerRepository.GetByIdAsync(new PlayerId(request.PlayerId), cancellationToken)
            ?? throw new NotFoundException(nameof(Player), request.PlayerId);

        player.LinkAuthProvider(
            provider: request.Provider,
            externalId: request.ExternalId,
            email: request.Email,
            linkedAt: DateTime.UtcNow);
    }
}
