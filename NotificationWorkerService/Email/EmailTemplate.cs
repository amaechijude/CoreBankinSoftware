using KafkaMessages.AccountMessages;

namespace NotificationWorkerService.Email;

internal static class EmailTemplateGenerator
{
    internal static EmailRequest CreateEmailRequests(TransactionAccountEvent transactionEvent)
    {
        return transactionEvent.EventType switch
        {
            EventType.TransferCredit => CreateCreditEmailRequest(transactionEvent),
            EventType.TransferDebit => CreateDebitEmailRequest(transactionEvent),
            EventType.Withdrawal => CreateDebitEmailRequest(transactionEvent),
            EventType.Deposit => CreateCreditEmailRequest(transactionEvent),
            EventType.Debit => CreateDebitEmailRequest(transactionEvent),
            EventType.Credit => CreateCreditEmailRequest(transactionEvent),
            EventType.Utility => CreateDebitEmailRequest(transactionEvent),
            _ => throw new EmailOptionsException("Unsupported transaction event type"),
        };
    }

    private static EmailRequest CreateDebitEmailRequest(TransactionAccountEvent transactionEvent)
    {
        string subject = "Debit Transaction Alert";
        string body =
            $@"
            <p>We would like to inform you that a debit transaction of {transactionEvent.Amount:C} 
            has been made from your account ending in {transactionEvent.SendersAccountNumber[^4..]} on {transactionEvent.Timestamp:MMMM dd, yyyy}.</p>
            <p>If you did not authorize this transaction, please contact our customer service immediately.</p>
            <p>Thank you for banking with us.</p>
            <p>Sincerely,<br/>Your Bank</p>
        ";

        return new EmailRequest(
            Subject: subject,
            TargetEmailAddress: transactionEvent.Email,
            FullName: transactionEvent.SendersAccountName,
            Body: body
        );
    }

    private static EmailRequest CreateCreditEmailRequest(TransactionAccountEvent transactionEvent)
    {
        string subject = "Credit Transaction Alert";
        string body =
            $@"
            <p>We are pleased to inform you that a credit transaction of {transactionEvent.Amount:C} 
            has been made to your account ending in {transactionEvent.SendersAccountNumber[^4..]} on {transactionEvent.Timestamp:MMMM dd, yyyy}.</p>
            <p>Thank you for banking with us.</p>
            <p>Sincerely,<br/>Your Bank</p>
        ";

        return new EmailRequest(
            Subject: subject,
            TargetEmailAddress: transactionEvent.Email,
            FullName: transactionEvent.SendersAccountName,
            Body: body
        );
    }
}
