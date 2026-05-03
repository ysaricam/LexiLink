using MediatR;

namespace LexiLink.Common.Domain;

public interface IDomainEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
