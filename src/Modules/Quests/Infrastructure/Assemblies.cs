using System.Reflection;
using LexiLink.Modules.Quests.Application.Contracts;

namespace LexiLink.Modules.Quests.Infrastructure;

internal static class Assemblies
{
    public static readonly Assembly Application = typeof(ICommand).Assembly;
}
