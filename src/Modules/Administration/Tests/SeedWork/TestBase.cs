using LexiLink.Common.Domain;

namespace LexiLink.Modules.Administration.Tests.SeedWork;

public abstract class TestBase
{
    protected static T AssertPublishedDomainEvent<T>(Entity aggregate) where T : IDomainEvent
    {
        var domainEvent = DomainEventsTestHelper.GetAllDomainEvents(aggregate).OfType<T>().SingleOrDefault();
        if (domainEvent is null)
            throw new AssertionException($"{typeof(T).Name} was not published.");
        return domainEvent;
    }

    protected static void AssertDomainEventNotPublished<T>(Entity aggregate) where T : IDomainEvent
    {
        var domainEvent = DomainEventsTestHelper.GetAllDomainEvents(aggregate).OfType<T>().SingleOrDefault();
        if (domainEvent is not null)
            throw new AssertionException($"{typeof(T).Name} was published but it shouldn't be.");
    }

    protected static void AssertBrokenRule<TRule>(TestDelegate testDelegate)
        where TRule : class, IBusinessRule
    {
        var ex = Assert.Catch<BusinessRuleValidationException>(testDelegate);
        ex.Should().NotBeNull();
        ex!.BrokenRule.Should().BeOfType<TRule>();
    }
}
