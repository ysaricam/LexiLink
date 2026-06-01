using LexiLink.Modules.Ads.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Ads.Infrastructure.Configuration;

internal class AdsModule : IAdsModule
{
    private readonly ISender _sender;

    internal AdsModule(ISender sender)
    {
        _sender = sender;
    }

    public Task<TResult> ExecuteCommandAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default) =>
        _sender.Send(command, cancellationToken);

    public Task ExecuteCommandAsync(ICommand command, CancellationToken cancellationToken = default) =>
        _sender.Send(command, cancellationToken);

    public Task<TResult> ExecuteQueryAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default) =>
        _sender.Send(query, cancellationToken);
}
