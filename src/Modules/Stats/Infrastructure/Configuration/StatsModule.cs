using LexiLink.Modules.Stats.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Stats.Infrastructure.Configuration;

internal class StatsModule : IStatsModule
{
    private readonly ISender _sender;

    internal StatsModule(ISender sender)
    {
        _sender = sender;
    }

    public Task<TResult> ExecuteQueryAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default) =>
        _sender.Send(query, cancellationToken);
}
