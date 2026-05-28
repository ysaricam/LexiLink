using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.Configuration.Verification;

public sealed class AppleIapOptions
{
    public const string SectionName = "Payments:Apple";

    public string BundleId { get; set; } = string.Empty;
    public PaymentEnvironment Environment { get; set; } = PaymentEnvironment.Sandbox;
    public string IssuerId { get; set; } = string.Empty;
    public string KeyId { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
}

public sealed class GooglePlayIapOptions
{
    public const string SectionName = "Payments:Google";

    public string PackageName { get; set; } = string.Empty;
    public PaymentEnvironment Environment { get; set; } = PaymentEnvironment.Sandbox;
    public string ServiceAccountJson { get; set; } = string.Empty;
}
