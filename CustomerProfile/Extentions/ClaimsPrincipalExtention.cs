using System.Security.Claims;

namespace CustomerProfile.Extentions;

public static class ClaimsPrincipalExtention
{
    public static Guid? GetValidId(this ClaimsPrincipal principal)
    {
        var claimId = principal.FindFirst(ClaimTypes.Sid)?.Value;
        return Guid.TryParse(claimId, out Guid id) ? id : null;
    }

    public static string? GetValidRole(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Role)?.Value;
    }
}
