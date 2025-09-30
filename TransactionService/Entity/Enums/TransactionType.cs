namespace TransactionService.Entity;

public enum TransactionType
{
    Debit,
    Credit
}

public enum TransactionChannel
{
    MobileApp,
    ATM,
    USSD,
    CreditCard
}
