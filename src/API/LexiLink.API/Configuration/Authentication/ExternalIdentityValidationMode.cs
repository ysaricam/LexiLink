namespace LexiLink.API.Configuration.Authentication;

public enum ExternalIdentityValidationMode
{
    Disabled = 0,
    DevelopmentExternalToken = 1,

    // Production-safe guest-only path: accepts the Guest provider (device-bound
    // identity) and rejects Apple/Google until real social sign-in is wired.
    // Allowed in Production (unlike DevelopmentExternalToken).
    GuestDevice = 2,

    // Production-safe player identity path: accepts GuestDevice plus verified
    // Google/Apple ID tokens for configured client ids.
    GuestDeviceAndSocial = 4,

    // Production-safe admin bootstrap path: accepts an operator-owned shared
    // external token before issuing an admin JWT. The email must still map to
    // an active AdminUser.
    AdminSharedSecret = 3
}
