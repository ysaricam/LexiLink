using LexiLink.Modules.Ads.Application.Configuration.Commands;
using LexiLink.Modules.Ads.Application.Contracts;

namespace LexiLink.Modules.Ads.Infrastructure.Configuration.Processing;

internal class UnitOfWorkCommandHandlerDecorator<T> : ICommandHandler<T>
    where T : ICommand
{
    private readonly ICommandHandler<T> _decorated;
    private readonly AdsUnitOfWork _unitOfWork;

    public UnitOfWorkCommandHandlerDecorator(
        ICommandHandler<T> decorated,
        AdsUnitOfWork unitOfWork)
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
