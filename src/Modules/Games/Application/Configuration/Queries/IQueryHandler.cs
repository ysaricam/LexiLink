using LexiLink.Modules.Games.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Games.Application.Configuration.Queries;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}