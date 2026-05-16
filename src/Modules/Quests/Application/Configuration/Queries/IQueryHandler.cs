using LexiLink.Modules.Quests.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Quests.Application.Configuration.Queries;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
