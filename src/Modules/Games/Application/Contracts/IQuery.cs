using MediatR;

namespace LexiLink.Modules.Games.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{}