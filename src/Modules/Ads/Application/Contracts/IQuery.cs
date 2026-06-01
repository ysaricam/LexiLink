using MediatR;

namespace LexiLink.Modules.Ads.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
