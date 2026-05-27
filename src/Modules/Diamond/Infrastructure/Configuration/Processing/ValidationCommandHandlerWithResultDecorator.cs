using FluentValidation;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Diamond.Application.Configuration.Commands;
using LexiLink.Modules.Diamond.Application.Contracts;

namespace LexiLink.Modules.Diamond.Infrastructure.Configuration.Processing;

internal class ValidationCommandHandlerWithResultDecorator<T, TResult> : ICommandHandler<T, TResult>
    where T : ICommand<TResult>
{
    private readonly IList<IValidator<T>> _validators;
    private readonly ICommandHandler<T, TResult> _decorated;

    public ValidationCommandHandlerWithResultDecorator(
        IList<IValidator<T>> validators,
        ICommandHandler<T, TResult> decorated)
    {
        _validators = validators;
        _decorated = decorated;
    }

    public Task<TResult> Handle(T command, CancellationToken cancellationToken)
    {
        var errors = _validators
            .Select(v => v.Validate(command))
            .SelectMany(result => result.Errors)
            .Where(error => error != null)
            .ToList();

        if (errors.Any())
        {
            throw new InvalidCommandException(errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray()));
        }

        return _decorated.Handle(command, cancellationToken);
    }
}
