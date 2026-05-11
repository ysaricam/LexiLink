using LexiLink.Modules.Players.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Players.Infrastructure.Configuration;

internal class PlayersModule : IPlayersModule
{
    private readonly ISender _sender;

    internal PlayersModule(ISender sender)
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
