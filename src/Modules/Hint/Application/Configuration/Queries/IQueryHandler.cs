using LexiLink.Modules.Hint.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Hint.Application.Configuration.Queries;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
