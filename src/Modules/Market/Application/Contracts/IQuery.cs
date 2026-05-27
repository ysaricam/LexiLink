using MediatR;

namespace LexiLink.Modules.Market.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
    Guid Id { get; }
}
