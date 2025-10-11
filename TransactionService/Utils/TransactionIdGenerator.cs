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
    /// <param name="senderBankCode">3-character sender's bank code</param>
    /// <param name="destinationBankCode">3-character destination bank code</param>
    /// <returns>30-character transaction ID</returns>
    public static string GenerateSessionId(string senderBankCode, string destinationBankCode) =>
        GenerateTransactionId(senderBankCode, destinationBankCode);


    /// <summary>
    /// Generates a RecID for bank transactions
    /// </summary>
    /// <param name="senderBankCode">3-character sender's bank code</param>
    /// <param name="destinationBankCode">3-character destination bank code</param>
    /// <returns>30-character transaction ID</returns>
    public static string GenerateRecId(string senderBankCode, string destinationBankCode) =>
        GenerateTransactionId(senderBankCode, destinationBankCode);

    /// <summary>
    /// Generates a random numeric string of specified length.
    /// </summary>
    /// <param name="length">The length of the numeric string to generate</param>
    /// <returns>A random numeric string of the specified length</returns>
    private static string GenerateUniqueNumber(int length)
    {
        var random = new Random();
        var sb = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            sb.Append(random.Next(0, 10));
        }

        return sb.ToString();
    }
    /// <summary>
    /// Generates a transaction ID using the specified bank codes and current timestamp.
    /// </summary>
    /// <param name="senderBankCode">3-character sender's bank code</param>
    /// <param name="destinationBankCode">3-character destination bank code</param>
    /// <returns>30-character transaction ID in the format: [SenderBank(3)][DestBank(3)][DateTime(12)][Random(12)]</returns>
    private static string GenerateTransactionId(string senderBankCode, string destinationBankCode)
    {

        var sb = new StringBuilder(30);

        // Char 1-3: Sender's bank code
        sb.Append(senderBankCode[..3].ToUpper());

        // Char 4-6: Destination bank code
        sb.Append(destinationBankCode[..3].ToUpper());

        // Char 7-18: Date and time (yymmddHHmmss)
        DateTime now = GetLagosTimeStamp();
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
        TimeZoneInfo lagosTimeZone = TimeZoneInfo
            .FindSystemTimeZoneById(TimezoneIdsStatic.W_Central_Africa_Standard_Time);
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lagosTimeZone);
    }
}

