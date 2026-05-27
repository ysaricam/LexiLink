using LexiLink.Modules.Market.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Market.Application.Configuration.Queries;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
