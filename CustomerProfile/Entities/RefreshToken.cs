using System.Security.Cryptography;

namespace CustomerProfile.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; init; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset Expires { get; set; }
    public DateTimeOffset Created { get; init; }
    public string CreatedByIp { get; init; } = string.Empty;
    public DateTimeOffset? Revoked { get; set; }
    public bool IsExpired => DateTimeOffset.UtcNow >= Expires;
    public bool IsActive => Revoked == null && !IsExpired;
    public Guid UserId { get; init; }

    public static RefreshToken Create(Guid userId, string createdByIp)
    {
        return new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            Token = GenerateRefreshToken(),
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Created = DateTimeOffset.UtcNow,
            CreatedByIp = createdByIp,
            UserId = userId,
        };
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public void Refresh()
    {
        Token = GenerateRefreshToken();
        Expires = DateTimeOffset.UtcNow.AddDays(7);
        Revoked = null;
    }

    public void Revoke()
    {
        Revoked = DateTimeOffset.UtcNow;
    }
}
