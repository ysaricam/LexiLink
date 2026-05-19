using System.Reflection;
using LexiLink.Modules.Administration.Application.Contracts;

namespace LexiLink.Modules.Administration.Infrastructure;

internal static class Assemblies
{
    public static readonly Assembly Application = typeof(ICommand).Assembly;
}
