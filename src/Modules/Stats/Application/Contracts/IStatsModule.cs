namespace LexiLink.Modules.Stats.Application.Contracts;

public interface IStatsModule
{
    Task<TResult> ExecuteQueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
