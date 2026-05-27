using MediatR;

namespace LexiLink.Modules.Diamond.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
