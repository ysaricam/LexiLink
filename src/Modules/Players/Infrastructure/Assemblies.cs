using System.Reflection;
using LexiLink.Modules.Players.Application.Contracts;

namespace LexiLink.Modules.Players.Infrastructure;

internal static class Assemblies
{
    public static readonly Assembly Application = typeof(ICommand).Assembly;
}
