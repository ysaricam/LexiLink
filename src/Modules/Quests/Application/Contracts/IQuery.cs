using MediatR;

namespace LexiLink.Modules.Quests.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
