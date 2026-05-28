using MediatR;

namespace LexiLink.Modules.Payments.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
    Guid Id { get; }
}
