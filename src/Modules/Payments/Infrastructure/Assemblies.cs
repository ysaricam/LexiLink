using System.Reflection;

namespace LexiLink.Modules.Payments.Infrastructure;

internal static class Assemblies
{
    internal static readonly Assembly Application =
        typeof(Application.Contracts.IPaymentsModule).Assembly;

    internal static readonly Assembly Infrastructure =
        typeof(Assemblies).Assembly;
}
