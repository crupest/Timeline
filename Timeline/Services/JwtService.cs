using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
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

    [Serializable]
    public class JwtTokenVerifyException : Exception
    {
        public JwtTokenVerifyException() { }
        public JwtTokenVerifyException(string message) : base(message) { }
        public JwtTokenVerifyException(string message, Exception inner) : base(message, inner) { }
        protected JwtTokenVerifyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
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
        /// <exception cref="JwtTokenVerifyException">Thrown when the token is invalid.</exception>
        TokenInfo VerifyJwtToken(string token);

    }

    public class JwtService : IJwtService
    {
        private const string VersionClaimType = "timeline_version";

        private readonly IOptionsMonitor<JwtConfig> _jwtConfig;
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();

        public JwtService(IOptionsMonitor<JwtConfig> jwtConfig)
        {
            _jwtConfig = jwtConfig;
        }

        public string GenerateJwtToken(TokenInfo tokenInfo, DateTime? expires = null)
        {
            if (tokenInfo == null)
                throw new ArgumentNullException(nameof(tokenInfo));

            var config = _jwtConfig.CurrentValue;

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, tokenInfo.Id.ToString(), ClaimValueTypes.Integer64));
            identity.AddClaim(new Claim(VersionClaimType, tokenInfo.Version.ToString(), ClaimValueTypes.Integer64));

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = identity,
                Issuer = config.Issuer,
                Audience = config.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.SigningKey)), SecurityAlgorithms.HmacSha384),
                IssuedAt = DateTime.Now,
                Expires = expires.GetValueOrDefault(DateTime.Now.AddSeconds(config.DefaultExpireOffset))
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
                    throw new JwtTokenVerifyException("Id claim does not exist.");
                if (!long.TryParse(idClaim, out var id))
                    throw new JwtTokenVerifyException("Can't convert id claim into a integer number.");

                var versionClaim = principal.FindFirstValue(VersionClaimType);
                if (versionClaim == null)
                    throw new JwtTokenVerifyException("Version claim does not exist.");
                if (!long.TryParse(versionClaim, out var version))
                    throw new JwtTokenVerifyException("Can't convert version claim into a integer number.");

                return new TokenInfo
                {
                    Id = id,
                    Version = version
                };
            }
            catch (SecurityTokenException e)
            {
                throw new JwtTokenVerifyException("Validate token failed caused by a SecurityTokenException. See inner exception.", e);
            }
        }
    }
}
