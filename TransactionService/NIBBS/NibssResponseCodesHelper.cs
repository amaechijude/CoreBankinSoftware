using TransactionService.Entity.Enums;

namespace TransactionService.NIBBS;

/// <summary>
/// Provides static methods for retrieving human-readable messages
/// based on transaction response codes.
/// </summary>
public static class NibssResponseCodesHelper
{
    /// <summary>
    /// Retrieves the appropriate description message for a given response code string.
    /// </summary>
    /// <param name="responseCode">The two-digit response code (e.g., "00", "51").</param>
    /// <returns>The corresponding message, or "Unknown response code" if no match is found.</returns>
    public static string GetMessageForCode(string responseCode)
    {
        // Do not edit this except if there is changes from Nibss documentation.
        return responseCode switch
        {
            // Successful responses
            "00" => "Approved or completed successfully",

            // General failures
            "01" => "Invalid sender",
            "05" => "Do not honor",
            "06" => "Dormant Account",
            "07" => "Invalid Account",
            "08" => "Account Name Mismatch",
            "09" => "Request processing in progress",
            "12" => "Invalid transaction",
            "13" => "Invalid Amount",
            "14" => "Invalid Batch Number",
            "15" => "Invalid Session or Record ID",
            "16" => "Unknown Bank Code",
            "17" => "Invalid Channel",
            "18" => "Wrong Method Call",
            "21" => "No action taken",
            "25" => "Unable to locate record",
            "26" => "Duplicate record",
            "31" => "Format error",
            "34" => "Suspected fraud",
            "35" => "Contact sending bank",

            // Specific financial/limit errors
            "51" => "No sufficient funds",
            "57" => "TransactionData not permitted to sender",
            "58" => "TransactionData not permitted on channel",
            "61" => "Transfer limit Exceeded",
            "63" => "Security violation",
            "65" => "Exceeds withdrawal frequency",

            // System/timing errors
            "68" => "Response received too late",
            "91" => "Beneficiary Bank not available",
            "92" => "Routing error",
            "94" => "Duplicate transaction",
            "96" => "System malfunction",
            "97" => "Timeout waiting for response from destination",

            // Default case if the code is not recognized
            _ => "Unknown response code"
        };
    }

    //Switch on transaction status
    public static TransactionStatus GetTransactionStatus(string responseCode)
    {
        return responseCode switch
        {
            // Success
            "00" => TransactionStatus.Completed,

            // In Progress
            "09" => TransactionStatus.Processing,

            // Declined by business rules (can be retried by user with different parameters)
            "05" => TransactionStatus.Declined, // Do not honor
            "06" => TransactionStatus.Declined, // Dormant Account
            "57" => TransactionStatus.Declined, // Transaction not permitted to sender
            "58" => TransactionStatus.Declined, // Transaction not permitted on channel
            "61" => TransactionStatus.Declined, // Transfer limit Exceeded
            "65" => TransactionStatus.Declined, // Exceeds withdrawal frequency

            // Blocked for security reasons
            "34" => TransactionStatus.Blocked,  // Suspected fraud
            "63" => TransactionStatus.Blocked,  // Security violation

            // Hard Failures (system/validation issues, not typically user-correctable)
            "51" => TransactionStatus.Failed,
            "01" => TransactionStatus.Failed,
            "07" => TransactionStatus.Failed,
            "08" => TransactionStatus.Failed,
            "12" => TransactionStatus.Failed,
            "13" => TransactionStatus.Failed,
            "26" => TransactionStatus.Failed, // Duplicate record
            "94" => TransactionStatus.Failed, // Duplicate transaction
            "91" => TransactionStatus.Failed, // Beneficiary Bank not available
            "96" => TransactionStatus.Failed, // System malfunction
            _ => TransactionStatus.Failed,
        };
    }
}