using LexiLink.Modules.Players.Application.Configuration.Queries;
using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.Modules.Players.Application.Admin.GetPlayerAdminDetail;

internal sealed class GetPlayerAdminDetailQueryHandler : IQueryHandler<GetPlayerAdminDetailQuery, PlayerAdminDetailDto?>
{
    private readonly IPlayerRepository _repository;

    internal GetPlayerAdminDetailQueryHandler(IPlayerRepository repository)
    {
        _repository = repository;
    }

    public async Task<PlayerAdminDetailDto?> Handle(
        GetPlayerAdminDetailQuery request,
        CancellationToken cancellationToken)
    {
        var player = await _repository.GetByIdAsync(
            new PlayerId(request.PlayerId),
            cancellationToken);

        if (player is null)
        {
            return null;
        }

        // Social providers only — Guest auth is not a "linked" external
        // provider in the product sense.
        var socialProviders = player.AuthIdentities
            .Count(a => a.Provider != AuthProvider.Guest);

        return new PlayerAdminDetailDto(
            Id: player.Id.Value,
            DisplayName: player.DisplayName,
            Discriminator: player.Discriminator.Value,
            Handle: $"{player.DisplayName}#{player.Discriminator}",
            AvatarUrl: player.AvatarUrl,
            Locale: player.Locale,
            IsGuest: player.IsGuest,
            IsBanned: player.IsBanned,
            BannedReason: player.BannedReason,
            BannedAt: player.BannedAt,
            CreatedAt: player.CreatedAt,
            AuthProvidersLinked: socialProviders);
    }
}
