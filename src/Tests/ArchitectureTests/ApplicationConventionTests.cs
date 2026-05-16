namespace LexiLink.ArchitectureTests;

[TestFixture]
public class ApplicationConventionTests : ArchitectureTestBase
{
    [TestCaseSource(nameof(ApplicationAssemblies))]
    public void ApplicationHandlers_Should_BeInternal(System.Reflection.Assembly assembly)
    {
        var publicHandlers = GetTypes(assembly)
            .Where(type => type.Name.EndsWith("Handler", StringComparison.Ordinal))
            .Where(type => type.IsPublic)
            .Select(type => type.FullName)
            .ToArray();

        publicHandlers.Should().BeEmpty();
    }

    [TestCaseSource(nameof(ApplicationAssemblies))]
    public void ApplicationValidators_Should_BeInternal(System.Reflection.Assembly assembly)
    {
        var publicValidators = GetTypes(assembly)
            .Where(IsValidator)
            .Where(type => type.IsPublic)
            .Select(type => type.FullName)
            .ToArray();

        publicValidators.Should().BeEmpty();
    }

    [TestCaseSource(nameof(ApplicationAssemblies))]
    public void CommandsAndQueries_Should_NotExposePublicSetters(System.Reflection.Assembly assembly)
    {
        var mutableRequestProperties = GetTypes(assembly)
            .Where(IsCommandOrQuery)
            .SelectMany(type => type
                .GetProperties()
                .Where(property => property.SetMethod?.IsPublic == true)
                .Select(property => $"{type.FullName}.{property.Name}"))
            .ToArray();

        mutableRequestProperties.Should().BeEmpty();
    }

    [TestCaseSource(nameof(ApplicationAssemblies))]
    public void RequestHandlers_Should_UseModuleHandlerContracts(System.Reflection.Assembly assembly)
    {
        var rawMediatRHandlers = GetTypes(assembly)
            .Where(ImplementsMediatRRequestHandler)
            .Where(type => !ImplementsModuleRequestHandlerContract(type))
            .Select(type => type.FullName)
            .ToArray();

        rawMediatRHandlers.Should().BeEmpty();
    }

    [TestCaseSource(nameof(ApplicationAssemblies))]
    public void InternalCommands_Should_HavePublicParameterlessConstructor(System.Reflection.Assembly assembly)
    {
        var invalidInternalCommands = GetTypes(assembly)
            .Where(IsInternalCommand)
            .Where(type => type.GetConstructor(Type.EmptyTypes) is null)
            .Select(type => type.FullName)
            .ToArray();

        invalidInternalCommands.Should().BeEmpty();
    }

    private static IEnumerable<System.Reflection.Assembly> ApplicationAssemblies()
    {
        yield return GamesApplicationAssembly;
        yield return PlayersApplicationAssembly;
        yield return StatsApplicationAssembly;
    }

    private static bool IsCommandOrQuery(Type type) =>
        type.GetInterfaces().Any(typeInterface =>
            typeInterface.FullName is not null &&
            (typeInterface.FullName.EndsWith(".Contracts.ICommand", StringComparison.Ordinal) ||
             typeInterface.FullName.Contains(".Contracts.ICommand`1", StringComparison.Ordinal) ||
             typeInterface.FullName.Contains(".Contracts.IQuery`1", StringComparison.Ordinal)));

    private static bool IsValidator(Type type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.FullName is not null &&
                current.FullName.StartsWith("FluentValidation.AbstractValidator`1", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ImplementsMediatRRequestHandler(Type type) =>
        type.GetInterfaces().Any(typeInterface =>
            typeInterface.FullName is not null &&
            typeInterface.FullName.StartsWith("MediatR.IRequestHandler", StringComparison.Ordinal));

    private static bool ImplementsModuleRequestHandlerContract(Type type) =>
        type.GetInterfaces().Any(typeInterface =>
            typeInterface.FullName is not null &&
            (typeInterface.FullName.Contains(".Application.Configuration.Commands.ICommandHandler", StringComparison.Ordinal) ||
             typeInterface.FullName.Contains(".Application.Configuration.Queries.IQueryHandler", StringComparison.Ordinal)));

    private static bool IsInternalCommand(Type type) =>
        type.GetInterfaces().Any(typeInterface =>
            typeInterface.FullName is not null &&
            typeInterface.FullName.EndsWith(".Application.Configuration.InternalCommands.IInternalCommand", StringComparison.Ordinal));
}
