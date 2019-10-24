using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Timeline.Configs;

namespace Timeline.Services
{
    public class TokenInfo
    {
        public long Id { get; set; }
        public long Version { get; set; }
    }

    public interface IJwtService
    {
        /// <summary>
        /// Create a JWT token for a given token info.
        /// </summary>
        /// <param name="tokenInfo">The info to generate token.</param>
        /// <param name="expires">The expire time. If null then use current time with offset in config.</param>
        /// <returns>Return the generated token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokenInfo"/> is null.</exception>
        string GenerateJwtToken(TokenInfo tokenInfo, DateTime? expires = null);

        /// <summary>
        /// Verify a JWT token.
        /// Return null is <paramref name="token"/> is null.
        /// </summary>
        /// <param name="token">The token string to verify.</param>
        /// <returns>Return the saved info in token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="token"/> is null.</exception>
        /// <exception cref="JwtVerifyException">Thrown when the token is invalid.</exception>
        TokenInfo VerifyJwtToken(string token);

    }

    public class JwtService : IJwtService
    {
        private const string VersionClaimType = "timeline_version";

        private readonly IOptionsMonitor<JwtConfig> _jwtConfig;
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
        private readonly IClock _clock;

        public JwtService(IOptionsMonitor<JwtConfig> jwtConfig, IClock clock)
        {
            _jwtConfig = jwtConfig;
            _clock = clock;
        }

        public string GenerateJwtToken(TokenInfo tokenInfo, DateTime? expires = null)
        {
            if (tokenInfo == null)
                throw new ArgumentNullException(nameof(tokenInfo));

            var config = _jwtConfig.CurrentValue;

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, tokenInfo.Id.ToString(CultureInfo.InvariantCulture.NumberFormat), ClaimValueTypes.Integer64));
            identity.AddClaim(new Claim(VersionClaimType, tokenInfo.Version.ToString(CultureInfo.InvariantCulture.NumberFormat), ClaimValueTypes.Integer64));

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = identity,
                Issuer = config.Issuer,
                Audience = config.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.SigningKey)), SecurityAlgorithms.HmacSha384),
                IssuedAt = _clock.GetCurrentTime(),
                Expires = expires.GetValueOrDefault(_clock.GetCurrentTime().AddSeconds(config.DefaultExpireOffset)),
                NotBefore = _clock.GetCurrentTime() // I must explicitly set this or it will use the current time by default and mock is not work in which case test will not pass.
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = _tokenHandler.WriteToken(token);

            return tokenString;
        }


        public TokenInfo VerifyJwtToken(string token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            var config = _jwtConfig.CurrentValue;
            try
            {
                var principal = _tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = config.Issuer,
                    ValidAudience = config.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.SigningKey))
                }, out _);

                var idClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (idClaim == null)
                    throw new JwtVerifyException(JwtVerifyException.ErrorCodes.NoIdClaim);
                if (!long.TryParse(idClaim, out var id))
                    throw new JwtVerifyException(JwtVerifyException.ErrorCodes.IdClaimBadFormat);

                var versionClaim = principal.FindFirstValue(VersionClaimType);
                if (versionClaim == null)
                    throw new JwtVerifyException(JwtVerifyException.ErrorCodes.NoVersionClaim);
                if (!long.TryParse(versionClaim, out var version))
                    throw new JwtVerifyException(JwtVerifyException.ErrorCodes.VersionClaimBadFormat);

                return new TokenInfo
                {
                    Id = id,
                    Version = version
                };
            }
            catch (SecurityTokenExpiredException e)
            {
                throw new JwtVerifyException(e, JwtVerifyException.ErrorCodes.Expired);
            }
            catch (Exception e)
            {
                throw new JwtVerifyException(e, JwtVerifyException.ErrorCodes.Others);
            }
        }
    }
}
