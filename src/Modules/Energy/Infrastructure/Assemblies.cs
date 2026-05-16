using System.Reflection;
using LexiLink.Modules.Energy.Application.Contracts;

namespace LexiLink.Modules.Energy.Infrastructure;

internal static class Assemblies
{
    public static readonly Assembly Application = typeof(ICommand).Assembly;
}
