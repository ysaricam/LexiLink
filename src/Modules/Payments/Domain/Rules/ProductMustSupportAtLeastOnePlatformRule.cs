using LexiLink.Common.Domain;

namespace LexiLink.Modules.Payments.Domain.Rules;

internal sealed class ProductMustSupportAtLeastOnePlatformRule : IBusinessRule
{
    private readonly bool _isAppleAvailable;
    private readonly bool _isGoogleAvailable;

    internal ProductMustSupportAtLeastOnePlatformRule(bool isAppleAvailable, bool isGoogleAvailable)
    {
        _isAppleAvailable = isAppleAvailable;
        _isGoogleAvailable = isGoogleAvailable;
    }

    public bool IsBroken() => !_isAppleAvailable && !_isGoogleAvailable;

    public string Message => "Payment product must support at least one platform.";
}
