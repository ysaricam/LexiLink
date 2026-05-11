using MediatR;

namespace LexiLink.Modules.Players.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
