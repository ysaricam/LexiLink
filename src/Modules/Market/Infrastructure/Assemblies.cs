using System.Reflection;
using LexiLink.Modules.Market.Application.Contracts;

namespace LexiLink.Modules.Market.Infrastructure;

internal static class Assemblies
{
    public static readonly Assembly Application = typeof(ICommand).Assembly;
}
