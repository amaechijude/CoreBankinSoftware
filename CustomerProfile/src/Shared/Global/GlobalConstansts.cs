using System.Security.Cryptography;

namespace src.Shared.Global
{
    public class GlobalConstansts
    {
        // usee cryptography to generate a 7 digit code
        public static string GenerateVerificationCode()
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] randomNumber = new byte[4]; // 4 bytes will give us a number up to 2^32 - 1
            rng.GetBytes(randomNumber);

            // Use Math.Abs to handle negative numbers, then modulo to get 7 digits
            int code = Math.Abs(BitConverter.ToInt32(randomNumber, 0)) % 10_000_000;
            return code.ToString("D7"); // Format as a 7-digit string with leading zeros
        }
    }
}
