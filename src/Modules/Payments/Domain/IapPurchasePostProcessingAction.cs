namespace LexiLink.Modules.Payments.Domain;

public enum IapPurchasePostProcessingAction
{
    None = 0,
    AppleClientFinishTransaction = 1,
    GoogleAcknowledge = 2,
    GoogleConsume = 3
}
