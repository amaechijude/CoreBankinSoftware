namespace CustomerProfile.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; init; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset Expires { get; set; }
    public DateTimeOffset Created { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public DateTimeOffset? Revoked { get; set; }
    public bool IsExpired => DateTimeOffset.UtcNow >= Expires;
    public bool IsActive => Revoked == null && !IsExpired;

    // Foreign key
    public Guid UserId { get; set; }
}
