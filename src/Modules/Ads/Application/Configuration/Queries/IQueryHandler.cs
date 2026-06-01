using LexiLink.Modules.Ads.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Ads.Application.Configuration.Queries;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
