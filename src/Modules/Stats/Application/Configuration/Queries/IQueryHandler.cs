using LexiLink.Modules.Stats.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Stats.Application.Configuration.Queries;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
