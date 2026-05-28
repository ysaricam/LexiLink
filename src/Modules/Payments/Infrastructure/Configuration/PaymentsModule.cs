using LexiLink.Modules.Payments.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Payments.Infrastructure.Configuration;

public class PaymentsModule : IPaymentsModule
{
    private readonly ISender _sender;

    public PaymentsModule(ISender sender)
    {
        _sender = sender;
    }

    public Task ExecuteCommandAsync(ICommand command, CancellationToken cancellationToken = default) =>
        _sender.Send(command, cancellationToken);

    public Task<TResult> ExecuteCommandAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default) =>
        _sender.Send(command, cancellationToken);

    public Task<TResult> ExecuteQueryAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default) =>
        _sender.Send(query, cancellationToken);
}
