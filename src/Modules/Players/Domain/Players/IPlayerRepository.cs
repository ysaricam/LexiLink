using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players;

public interface IPlayerRepository : IRepository<Player>
{
    Task<Player?> GetByIdAsync(PlayerId id, CancellationToken cancellationToken = default);

    Task<Player?> GetByHandleAsync(string displayName, int discriminator, CancellationToken cancellationToken = default);

    Task<Player?> GetByAuthProviderAsync(AuthProvider provider, string externalId, CancellationToken cancellationToken = default);

    Task AddAsync(Player player, CancellationToken cancellationToken = default);
}
