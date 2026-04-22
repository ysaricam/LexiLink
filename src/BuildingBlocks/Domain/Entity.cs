namespace LexiLink.BuildingBlocks.Domain;

public abstract class Entity
{
    protected static void CheckRule(IBusinessRule rule)
    {
       if(rule.IsBroken())
           throw new BusinessRuleValidationException(rule);
    }
}

public abstract class AggregateRoot : Entity, IAggregateRoot
{
    private List<IDomainEvent>? _domainEvents;

    public IReadOnlyCollection<IDomainEvent>? DomainEvents => _domainEvents?.AsReadOnly();

    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents ??= new List<IDomainEvent>();
        _domainEvents.Add(domainEvent);
    }
}