using System.Reflection;
using LexiLink.Modules.Reset.Application.Contracts;

namespace LexiLink.Modules.Reset.Infrastructure;

internal static class Assemblies
{
    public static readonly Assembly Application = typeof(ICommand).Assembly;
}
