using MediatR;

namespace LexiLink.Modules.Payments.Application.Contracts;

public interface ICommand : IRequest
{
    Guid Id { get; }
}

public interface ICommand<out TResult> : IRequest<TResult>
{
    Guid Id { get; }
}
