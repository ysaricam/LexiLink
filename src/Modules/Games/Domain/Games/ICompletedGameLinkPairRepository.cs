using LexiLink.Modules.Games.Domain.Categories;

namespace LexiLink.Modules.Games.Domain.Games;

public interface ICompletedGameLinkPairRepository
{
    Task<IReadOnlyCollection<CompletedGameLinkPair>> GetCompletedPairsAsync(
        Guid playerId,
        CategoryId categoryId,
        CancellationToken cancellationToken = default);
}
