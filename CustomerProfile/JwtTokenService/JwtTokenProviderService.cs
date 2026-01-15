using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CustomerProfile.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CustomerProfile.JwtTokenService;

public sealed class JwtTokenProviderService(IOptions<JwtOptions> jwtOptions)
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public string GenerateVerificationResponseJwtToken(VerificationCode code)
    {
        var claims = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Sid, code.Id.ToString()),
                new Claim(ClaimTypes.Role, RolesUtils.VerificationRole),
                new Claim(ClaimTypes.Email, code.UserEmail),
            ]
        );
        return GetAccesToken(claims: claims, expiryInMinutes: 15 * 4);
    }

    public string GenerateUserJwtToken(UserProfile user)
    {
        var claims = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Sid, user.Id.ToString()),
                new Claim(ClaimTypes.Role, RolesUtils.UserRole),
            ]
        );
        return GetAccesToken(claims: claims, expiryInMinutes: 15);
    }

    public async Task<ClaimsPrincipal?> ValidateToken(string token)
    {
        JwtSecurityTokenHandler jsonWebTokenHandler = new();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));

        try
        {
            var param = new TokenValidationParameters
            {
                ValidIssuer = _jwtOptions.Issuer,
                ValidateIssuer = true,

                ValidAudience = _jwtOptions.Audience,
                ValidateAudience = true,

                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero,

                IssuerSigningKey = key,
                ValidateIssuerSigningKey = true,
            };
            var principal = jsonWebTokenHandler.ValidateToken(
                token,
                param,
                out SecurityToken validatedToken
            );

            var jwtToken = (JwtSecurityToken)validatedToken;

            // Verify algorithm
            if (
                !jwtToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase
                )
            )
            {
                return null;
            }
            return principal;
        }
        catch
        {
            return null;
        }
    }

    private string GetAccesToken(ClaimsIdentity claims, int expiryInMinutes)
    {
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = claims,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = signingCredentials,
            Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes),
            IssuedAt = DateTime.UtcNow,
        };

        JsonWebTokenHandler jsonWebTokenHandler = new();

        return jsonWebTokenHandler.CreateToken(tokenDescriptor);
    }
}
