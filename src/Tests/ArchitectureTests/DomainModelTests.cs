using LexiLink.Common.Domain;

namespace LexiLink.ArchitectureTests;

[TestFixture]
public class DomainModelTests : ArchitectureTestBase
{
    [Test]
    public void BusinessRules_Should_ImplementIBusinessRule()
    {
        var failingTypes = ModuleAssemblies
            .Where(assembly => assembly.GetName().Name!.EndsWith(".Domain"))
            .SelectMany(GetTypes)
            .Where(type => type.Name.EndsWith("Rule"))
            .Where(type => !typeof(IBusinessRule).IsAssignableFrom(type))
            .Select(type => type.FullName)
            .ToList();

        failingTypes.Should().BeEmpty();
    }

    [Test]
    public void DomainEvents_Should_EndWithDomainEvent()
    {
        var failingTypes = ModuleAssemblies
            .Where(assembly => assembly.GetName().Name!.EndsWith(".Domain"))
            .SelectMany(GetTypes)
            .Where(type => typeof(IDomainEvent).IsAssignableFrom(type))
            .Where(type => !type.Name.EndsWith("DomainEvent"))
            .Select(type => type.FullName)
            .ToList();

        failingTypes.Should().BeEmpty();
    }

    [Test]
    public void AggregateRoots_Should_ImplementIAggregateRoot()
    {
        var aggregateNames = new HashSet<string>
        {
            "Category",
            "Link",
            "Game",
            "Player",
            "AdminUser",
            "QuestDefinition"
        };

        var failingTypes = ModuleAssemblies
            .Where(assembly => assembly.GetName().Name!.EndsWith(".Domain"))
            .SelectMany(GetTypes)
            .Where(type => aggregateNames.Contains(type.Name))
            .Where(type => !typeof(IAggregateRoot).IsAssignableFrom(type))
            .Select(type => type.FullName)
            .ToList();

        failingTypes.Should().BeEmpty();
    }

    [Test]
    public void Entities_Should_NotHavePublicConstructors()
    {
        var failingTypes = ModuleAssemblies
            .Where(assembly => assembly.GetName().Name!.EndsWith(".Domain"))
            .SelectMany(GetTypes)
            .Where(type => typeof(Entity).IsAssignableFrom(type))
            .Where(type => type.GetConstructors().Any())
            .Select(type => type.FullName)
            .ToList();

        failingTypes.Should().BeEmpty();
    }
}
