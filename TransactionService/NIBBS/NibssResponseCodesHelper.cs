namespace TransactionService.NIBBS;

/// <summary>
/// Provides static methods for retrieving human-readable messages
/// based on transaction response codes.
/// </summary>
public static class ResponseCodeHelper
{
    /// <summary>
    /// Retrieves the appropriate description message for a given response code string.
    /// </summary>
    /// <param name="responseCode">The two-digit response code (e.g., "00", "51").</param>
    /// <returns>The corresponding message, or "Unknown response code" if no match is found.</returns>
    public static string GetMessageForCode(string responseCode)
    {
        // Normalize the input code by converting to uppercase and trimming whitespace
        // to ensure robustness, then use a switch expression for mapping.
        string code = responseCode?.Trim().ToUpperInvariant() ?? string.Empty;

        return code switch
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
            "57" => "Transaction not permitted to sender",
            "58" => "Transaction not permitted on channel",
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
}