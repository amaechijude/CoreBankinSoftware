using System.Security.Cryptography;

namespace src.Shared.Global
{
    public static class GlobalConstansts
    {
        // use cryptography to generate a 7 digit code
        public static string GenerateVerificationCode()
        {
            Span<byte> randomNumber = stackalloc byte[4];
            RandomNumberGenerator.Fill(randomNumber);

            // Mask to ensure 7 digits (0-9,999,999) without modulo bias
            uint code = BitConverter.ToUInt32(randomNumber) % 10_000_000u;
            return code.ToString("D7");
        }
    }


}
