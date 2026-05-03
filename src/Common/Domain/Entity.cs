namespace LexiLink.Common.Domain;

public abstract class Entity
{
    private List<IDomainEvent>? _domainEvents;

    public IReadOnlyCollection<IDomainEvent> DomainEvents
        => _domainEvents?.AsReadOnly() ?? (IReadOnlyCollection<IDomainEvent>)Array.Empty<IDomainEvent>();

    public void ClearDomainEvents() => _domainEvents?.Clear();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents ??= [];
        _domainEvents.Add(domainEvent);
    }

    protected static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
            throw new BusinessRuleValidationException(rule);
    }
}
