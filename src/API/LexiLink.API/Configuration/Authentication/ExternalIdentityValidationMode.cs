namespace LexiLink.API.Configuration.Authentication;

public enum ExternalIdentityValidationMode
{
    Disabled = 0,
    DevelopmentExternalToken = 1,

    // Production-safe guest-only path: accepts the Guest provider (device-bound
    // identity) and rejects Apple/Google until real social sign-in is wired.
    // Allowed in Production (unlike DevelopmentExternalToken).
    GuestDevice = 2
}
