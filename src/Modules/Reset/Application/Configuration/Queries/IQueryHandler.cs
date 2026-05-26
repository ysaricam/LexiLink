using LexiLink.Modules.Reset.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Reset.Application.Configuration.Queries;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
