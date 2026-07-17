namespace LexiLink.API.CrossModule;

public interface IPlayerDataDeletionService
{
    Task<bool> DeleteAsync(Guid playerId, CancellationToken cancellationToken = default);
}
