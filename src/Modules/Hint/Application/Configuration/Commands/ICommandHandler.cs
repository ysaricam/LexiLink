using LexiLink.Modules.Hint.Application.Contracts;
using MediatR;

namespace LexiLink.Modules.Hint.Application.Configuration.Commands;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand
{
}

public interface ICommandHandler<in TCommand, TResult> :
    IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
}
