using MediatR;

namespace LexiLink.Modules.Administration.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
