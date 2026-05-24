using MediatR;

namespace LexiLink.Modules.Hint.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
