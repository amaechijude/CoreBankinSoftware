using KafkaMessages.AccountMessages;

namespace Notification.Workers;

internal static class EmailTemplateGenerator
{
    internal static List<EmailRequest> CreateEmailRequests(TransactionAccountEvent transactionEvent)
    {
        var emailRequests = new List<EmailRequest>(2);

        if (transactionEvent.EventType == EventType.Transfer)
        {
            // Email for debit (source account)
            emailRequests.Add(
                new EmailRequest(
                    Subject: $"Transfer Debit - {transactionEvent.TransactionReference}",
                    TargetEmailAddress: GetCustomerEmail(transactionEvent.CustomerId),
                    FullName: GetCustomerName(transactionEvent.CustomerId),
                    Body: $"Amount {transactionEvent.Amount} has been debited from your account. "
                        + $"Transferred to {transactionEvent.DestinationBankName} - {transactionEvent.DestinationAccountNumber}. "
                        + $"Fee: {transactionEvent.TransactionFee}"
                )
            );

            // Email for credit (destination account)
            emailRequests.Add(
                new EmailRequest(
                    Subject: $"Transfer Credit - {transactionEvent.TransactionReference}",
                    TargetEmailAddress: GetDestinationCustomerEmail(
                        transactionEvent.DestinationAccountNumber
                    ),
                    FullName: GetDestinationCustomerName(transactionEvent.DestinationAccountNumber),
                    Body: $"Amount {transactionEvent.Amount} has been credited to your account. "
                        + $"From transaction reference: {transactionEvent.TransactionReference}"
                )
            );
        }
        else
        {
            // Single email for Credit, Debit, or Utility
            emailRequests.Add(
                new EmailRequest(
                    Subject: $"{transactionEvent.EventType} Transaction - {transactionEvent.TransactionReference}",
                    TargetEmailAddress: GetCustomerEmail(transactionEvent.CustomerId),
                    FullName: GetCustomerName(transactionEvent.CustomerId),
                    Body: $"Your account has been {transactionEvent.EventType.ToString().ToLower()}ed with amount {transactionEvent.Amount}. "
                        + $"Transaction reference: {transactionEvent.TransactionReference}"
                )
            );
        }

        return emailRequests;
    }

    private static string GetDestinationCustomerEmail(string destinationAccountNumber)
    {
        throw new NotImplementedException();
    }

    private static string GetCustomerName(Guid customerId)
    {
        throw new NotImplementedException();
    }

    private static string GetCustomerEmail(Guid customerId)
    {
        throw new NotImplementedException();
    }

    private static string GetDestinationCustomerName(string destinationAccountNumber)
    {
        throw new NotImplementedException();
    }
}
