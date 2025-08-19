using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CustomerAPI.JwtTokenService
{
    public class JwtTokenProviderService(IOptions<JwtOptions> jwtOptions)
    {
        private readonly JwtOptions _jwtOptions = jwtOptions.Value;

        public string GenerateVerificationResponseJwtToken(Guid Id)
        {
            string currentId = Id == Guid.Empty
                ? throw new JwtGenException($"Id cannot be empty: {nameof(Id)}")
                : Id.ToString();

            SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));

            SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new System.Security.Claims.ClaimsIdentity([
                    new Claim(ClaimTypes.Sid, currentId),
                    new Claim(ClaimTypes.Role, RolesUtils.VerificationRole)
                    ]),

                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = signingCredentials,
                
            };

            JsonWebTokenHandler jsonWebTokenHandler = new();

            return jsonWebTokenHandler
                .CreateToken(tokenDescriptor);
        }
    }

   internal sealed class JwtGenException(string message) : Exception(message);
}
