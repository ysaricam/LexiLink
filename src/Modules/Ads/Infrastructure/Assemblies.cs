using System.Reflection;
using LexiLink.Modules.Ads.Application.Contracts;

namespace LexiLink.Modules.Ads.Infrastructure;

internal static class Assemblies
{
    public static readonly Assembly Application = typeof(ICommand).Assembly;
}
