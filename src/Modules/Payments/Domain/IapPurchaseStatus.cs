namespace LexiLink.Modules.Payments.Domain;

public enum IapPurchaseStatus
{
    Received = 1,
    Verified = 2,
    Granted = 3,
    VerifiedButGrantFailed = 4,
    Failed = 5,
    Refunded = 6,
    Revoked = 7
}
