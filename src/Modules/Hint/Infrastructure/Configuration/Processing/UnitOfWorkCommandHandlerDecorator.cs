using LexiLink.Modules.Hint.Application.Configuration.Commands;
using LexiLink.Modules.Hint.Application.Contracts;

namespace LexiLink.Modules.Hint.Infrastructure.Configuration.Processing;

internal class UnitOfWorkCommandHandlerDecorator<T> : ICommandHandler<T>
    where T : ICommand
{
    private readonly ICommandHandler<T> _decorated;
    private readonly HintUnitOfWork _unitOfWork;

    public UnitOfWorkCommandHandlerDecorator(
        ICommandHandler<T> decorated,
        HintUnitOfWork unitOfWork)
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
