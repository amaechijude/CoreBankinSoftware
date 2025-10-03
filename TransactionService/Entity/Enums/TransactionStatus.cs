namespace TransactionService.Entity.Enums
{
    public enum TransactionStatus
    {
        Initiated,
        Validated,
        Authorized,
        Processing,
        Completed,
        Failed,
        Reversed,
        Blocked,
        Cancelled
    }
}