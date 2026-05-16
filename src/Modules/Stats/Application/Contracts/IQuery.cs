using MediatR;

namespace LexiLink.Modules.Stats.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
