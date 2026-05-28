namespace LexiLink.Modules.Payments.Domain;

public enum IapPurchasePostProcessingStatus
{
    None = 0,
    Pending = 1,
    Succeeded = 2,
    Failed = 3
}
