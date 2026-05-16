using LexiLink.Modules.Energy.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Energy.Application.Configuration.Queries;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
