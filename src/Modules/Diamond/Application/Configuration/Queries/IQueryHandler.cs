using LexiLink.Modules.Diamond.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Diamond.Application.Configuration.Queries;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
