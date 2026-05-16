using System.Reflection;
using LexiLink.Modules.Stats.Application.Contracts;

namespace LexiLink.Modules.Stats.Infrastructure;

internal static class Assemblies
{
    public static readonly Assembly Application = typeof(IStatsModule).Assembly;
}
