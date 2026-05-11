using LexiLink.Modules.Players.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Players.Application.Configuration.Queries;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
