using LexiLink.Modules.Administration.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Administration.Application.Configuration.Queries;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
