namespace TransactionService.Entity.Enums;

public enum TransactionType
{
    Debit,
    Credit,
    Transfer,
    Withdrawal,
    Deposit,
    BillPayment
}

public enum TransactionChannel
{
    MobileApp,
    ATM,
    USSD,
    CreditCard,
    BankTeller,
    Website,
    API,
    POS
}
