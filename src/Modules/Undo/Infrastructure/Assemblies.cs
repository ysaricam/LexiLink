using System.Reflection;
using LexiLink.Modules.Undo.Application.Contracts;

namespace LexiLink.Modules.Undo.Infrastructure;

internal static class Assemblies
{
    public static readonly Assembly Application = typeof(ICommand).Assembly;
}
