namespace POS.Domain.Enums;

public enum TransactionStatus
{
    Open,
    InProgress,
    PaymentPending,
    Completed,
    Voided,
    Refunded
}
