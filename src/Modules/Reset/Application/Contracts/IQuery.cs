using MediatR;

namespace LexiLink.Modules.Reset.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
