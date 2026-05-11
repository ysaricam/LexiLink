using LexiLink.Common.Infrastructure;
using LexiLink.Modules.Players.Application.Configuration.Commands;
using LexiLink.Modules.Players.Application.Contracts;

namespace LexiLink.Modules.Players.Infrastructure.Configuration.Processing;

internal class UnitOfWorkCommandHandlerWithResultDecorator<T, TResult> : ICommandHandler<T, TResult>
    where T : ICommand<TResult>
{
    private readonly ICommandHandler<T, TResult> _decorated;
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkCommandHandlerWithResultDecorator(
        ICommandHandler<T, TResult> decorated,
        IUnitOfWork unitOfWork)
    {
        _decorated = decorated;
        _unitOfWork = unitOfWork;
    }

    public async Task<TResult> Handle(T command, CancellationToken cancellationToken)
    {
        var result = await _decorated.Handle(command, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
        return result;
    }
}
