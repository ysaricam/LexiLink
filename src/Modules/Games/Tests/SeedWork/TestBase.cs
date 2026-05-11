using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Tests.SeedWork;

public abstract class TestBase
{
    protected static T AssertPublishedDomainEvent<T>(Entity aggregate) where T : IDomainEvent
    {
        var domainEvent = DomainEventsTestHelper.GetAllDomainEvents(aggregate).OfType<T>().SingleOrDefault();
        if (domainEvent is null)
            throw new AssertionException($"{typeof(T).Name} was not published.");
        return domainEvent;
    }

    protected static IReadOnlyList<T> AssertPublishedDomainEvents<T>(Entity aggregate) where T : IDomainEvent
    {
        var domainEvents = DomainEventsTestHelper.GetAllDomainEvents(aggregate).OfType<T>().ToList();
        if (domainEvents.Count == 0)
            throw new AssertionException($"{typeof(T).Name} was not published.");
        return domainEvents;
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

    protected static async Task AssertBrokenRuleAsync<TRule>(AsyncTestDelegate testDelegate)
        where TRule : class, IBusinessRule
    {
        var ex = Assert.CatchAsync<BusinessRuleValidationException>(testDelegate);
        ex.Should().NotBeNull();
        ex!.BrokenRule.Should().BeOfType<TRule>();
        await Task.CompletedTask;
    }
}
