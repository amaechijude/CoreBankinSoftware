using CustomerProfile.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace CustomerProfile.JwtTokenService
{
    public class JwtTokenProviderService(IOptions<JwtOptions> jwtOptions)
    {
        private readonly JwtOptions _jwtOptions = jwtOptions.Value;

        public (string token, DateTime? expiresIn) GenerateVerificationResponseJwtToken(VerificationCode code)
        {
            SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));

            SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new System.Security.Claims.ClaimsIdentity([
                    new Claim(ClaimTypes.Sid, code.Id.ToString()),
                    new Claim(ClaimTypes.Role, RolesUtils.VerificationRole),
                    new Claim(ClaimTypes.Email, code.UserEmail)
                    ]),

                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = signingCredentials,

            };

            JsonWebTokenHandler jsonWebTokenHandler = new();

            string token = jsonWebTokenHandler.CreateToken(tokenDescriptor);

            return (token, tokenDescriptor.Expires);
        }

        public (string token, DateTime? expiresIn) GenerateUserJwtToken(UserProfile user)
        {
            SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));

            SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new System.Security.Claims.ClaimsIdentity([
                    new Claim(ClaimTypes.Sid, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, RolesUtils.UserRole),
                    ]),

                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = signingCredentials,

            };

            JsonWebTokenHandler jsonWebTokenHandler = new();

            string token = jsonWebTokenHandler.CreateToken(tokenDescriptor);

            return (token, tokenDescriptor.Expires);
        }
    }

    internal sealed class JwtGenException(string message) : Exception(message);
}
