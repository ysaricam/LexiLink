using System.Reflection;
using LexiLink.Modules.Hint.Application.Contracts;

namespace LexiLink.Modules.Hint.Infrastructure;

internal static class Assemblies
{
    public static readonly Assembly Application = typeof(ICommand).Assembly;
}
