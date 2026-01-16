using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace CustomerProfile.Services;

public sealed class CryptographyService(IOptions<PiiSecurityOptions> options)
{
    private readonly byte[] _secretKey = Encoding.UTF8.GetBytes(options.Value.HashingKey);

    public string HashSensitiveData(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        using var hmac = new HMACSHA256(_secretKey);
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hashBytes);
    }
}

public sealed class PiiSecurityOptions
{
    [Required, MinLength(64)]
    public string HashingKey { get; set; } = string.Empty;
}
