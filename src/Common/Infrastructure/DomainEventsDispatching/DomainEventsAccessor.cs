using LexiLink.Common.Application.Events;
using LexiLink.Common.Domain;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Common.Infrastructure.DomainEventsDispatching;

public class DomainEventsAccessor : IDomainEventsAccessor
{
    private readonly DbContext _dbContext;

    public DomainEventsAccessor(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyCollection<IDomainEvent> GetAllDomainEvents()
    {
        var domainEntities = _dbContext.ChangeTracker
            .Entries<Entity>()
            .Where(d => d.Entity.DomainEvents != null && d.Entity.DomainEvents.Any()).ToList();
    
        return domainEntities
            .SelectMany(d => d.Entity.DomainEvents)
            .ToList();
    }

    public void ClearAllDomainEvents()
    {
        var domainEntities = _dbContext.ChangeTracker
            .Entries<Entity>()
            .Where(d => d.Entity.DomainEvents != null && d.Entity.DomainEvents.Any()).ToList();
    
        domainEntities
            .ForEach(entity => entity.Entity.ClearDomainEvents());
    }
}