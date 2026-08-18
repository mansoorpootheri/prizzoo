using Abp.Runtime.Security;
using EnterpriseBase.Authentication.JwtBearer;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace EnterpriseBase.Controllers
{
    /// <summary>
    /// Shared JWT-issuance plumbing for any controller that hands out an
    /// access token after authenticating a user - password login
    /// (TokenAuthController) and OTP login (OtpAuthController) both produce
    /// tokens the same way so they're interchangeable everywhere
    /// [AbpAuthorize] is used.
    /// </summary>
    public abstract class JwtIssuingControllerBase : EnterpriseBaseControllerBase
    {
        protected abstract TokenAuthConfiguration Configuration { get; }

        protected string CreateAccessToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: Configuration.Issuer,
                audience: Configuration.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(expiration ?? Configuration.Expiration),
                signingCredentials: Configuration.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        protected static List<Claim> CreateJwtClaims(ClaimsIdentity identity)
        {
            var claims = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            // Specifically add the jti (random nonce), iat (issued timestamp), and sub (subject/user) claims.
            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            });

            return claims;
        }

        protected static string GetEncryptedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken);
        }
    }
}
