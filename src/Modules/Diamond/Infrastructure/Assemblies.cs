using System.Reflection;
using LexiLink.Modules.Diamond.Application.Contracts;

namespace LexiLink.Modules.Diamond.Infrastructure;

internal static class Assemblies
{
    public static readonly Assembly Application = typeof(ICommand).Assembly;
}
