using LexiLink.Modules.Undo.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Undo.Application.Configuration.Queries;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
