namespace TransactionService.Entity.Enums;
public enum TransactionStatus
{
    Initiated,
    Processing,
    Completed,
    Declined,
    Failed,
    Reversed,
    Blocked,
    Cancelled
}
