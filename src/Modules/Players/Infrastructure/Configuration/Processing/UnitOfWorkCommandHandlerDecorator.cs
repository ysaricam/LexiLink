using LexiLink.Common.Infrastructure;
using LexiLink.Modules.Players.Application.Configuration.Commands;
using LexiLink.Modules.Players.Application.Contracts;

namespace LexiLink.Modules.Players.Infrastructure.Configuration.Processing;

internal class UnitOfWorkCommandHandlerDecorator<T> : ICommandHandler<T>
    where T : ICommand
{
    private readonly ICommandHandler<T> _decorated;
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkCommandHandlerDecorator(
        ICommandHandler<T> decorated,
        IUnitOfWork unitOfWork)
    {
        _decorated = decorated;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(T command, CancellationToken cancellationToken)
    {
        await _decorated.Handle(command, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
