using System.Text;

namespace TransactionService.Utils;

/// <summary>
/// Provides functionality for generating unique transaction identifiers for banking operations.
/// </summary>
internal static class TransactionIdGenerator
{
    /// <summary>
    /// Generates a Session ID for bank transactions
    /// </summary>
    public static string GenerateSessionId(string senderBankCode, string destinationBankCode) =>
        GenerateTransactionId(senderBankCode, destinationBankCode);

    /// <summary>
    /// Generates a RecID for bank transactions
    /// </summary>
    public static string GenerateRecId(string senderBankCode, string destinationBankCode) =>
        GenerateTransactionId(senderBankCode, destinationBankCode);

    /// <summary>
    /// Generates a random numeric string of specified length.
    /// </summary>
    private static string GenerateUniqueNumber(int length)
    {
        var random = new Random();
        var sb = new StringBuilder(length);

        for (var i = 0; i < length; i++)
        {
            sb.Append(random.Next(0, 10));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates a transaction ID using the specified bank codes and current timestamp.
    /// </summary>
    private static string GenerateTransactionId(string senderBankCode, string destinationBankCode)
    {
        var sb = new StringBuilder(30);

        // Char 1-3: Sender's bank code
        sb.Append(senderBankCode[..3].ToUpper());

        // Char 4-6: Destination bank code
        sb.Append(destinationBankCode[..3].ToUpper());

        // Char 7-18: Date and time (yymmddHHmmss)
        var now = GetLagosTimeStamp();
        sb.Append(now.ToString("yyMMddHHmmss"));

        // Char 19-30: 12-character unique number (random)
        sb.Append(GenerateUniqueNumber(12));

        return sb.ToString();
    }

    /// <summary>
    /// Gets the current timestamp in Lagos (West Africa Standard Time) timezone.
    /// </summary>
    /// <returns>Current DateTime in Lagos timezone</returns>
    private static DateTime GetLagosTimeStamp()
    {
        var lagosTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
            TimezoneIdsStatic.W_Central_Africa_Standard_Time
        );
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lagosTimeZone);
    }
}
