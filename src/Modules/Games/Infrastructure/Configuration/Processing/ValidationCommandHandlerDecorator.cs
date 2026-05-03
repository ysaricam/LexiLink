using FluentValidation;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Infrastructure.Configuration.Processing;

internal class ValidationCommandHandlerDecorator<T> : ICommandHandler<T>
    where T : ICommand
{
    private readonly IList<IValidator<T>> _validators;
    private readonly ICommandHandler<T> _decorated;

    public ValidationCommandHandlerDecorator(
        IList<IValidator<T>> validators,
        ICommandHandler<T> decorated)
    {
        _validators = validators;
        _decorated = decorated;
    }

    public async Task Handle(T command, CancellationToken cancellationToken)
    {
        var errors = _validators
            .Select(v => v.Validate(command))
            .SelectMany(result => result.Errors)
            .Where(error => error != null)
            .ToList();

        if (errors.Any())
        {
            throw new InvalidCommandException(errors.Select(x => x.ErrorMessage).ToList());
        }

        await _decorated.Handle(command, cancellationToken);
    }
}
