using MediatR;

namespace LexiLink.Modules.Undo.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
