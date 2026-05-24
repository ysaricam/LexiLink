using LexiLink.Modules.Hint.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Hint.Infrastructure.Configuration;

internal class HintModule : IHintModule
{
    private readonly ISender _sender;

    internal HintModule(ISender sender)
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
