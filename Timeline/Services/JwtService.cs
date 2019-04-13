using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Timeline.Configs;

namespace Timeline.Services
{
    public interface IJwtService
    {
        /// <summary>
        /// Create a JWT token for a given user id.
        /// </summary>
        /// <param name="userId">The user id used to generate token.</param>
        /// <returns>Return the generated token.</returns>
        string GenerateJwtToken(long userId, string[] roles);

        /// <summary>
        /// Verify a JWT token.
        /// Return null is <paramref name="token"/> is null.
        /// </summary>
        /// <param name="token">The token string to verify.</param>
        /// <returns>Return null if <paramref name="token"/> is null or token is invalid. Return the saved user id otherwise.</returns>
        long? VerifyJwtToken(string token);

    }

    public class JwtService : IJwtService
    {
        private readonly IOptionsMonitor<JwtConfig> _jwtConfig;
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
        private readonly ILogger<JwtService> _logger;

        public JwtService(IOptionsMonitor<JwtConfig> jwtConfig, ILogger<JwtService> logger)
        {
            _jwtConfig = jwtConfig;
            _logger = logger;
        }

        public string GenerateJwtToken(long id, string[] roles)
        {
            var jwtConfig = _jwtConfig.CurrentValue;

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, id.ToString()));
            identity.AddClaims(roles.Select(role => new Claim(identity.RoleClaimType, role)));

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = identity,
                Issuer = jwtConfig.Issuer,
                Audience = jwtConfig.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfig.SigningKey)), SecurityAlgorithms.HmacSha384),
                IssuedAt = DateTime.Now,
                Expires = DateTime.Now.AddDays(1)
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = _tokenHandler.WriteToken(token);

            return tokenString;
        }


        public long? VerifyJwtToken(string token)
        {
            if (token == null)
                return null;

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
                }, out SecurityToken validatedToken);

                return long.Parse(principal.FindAll(ClaimTypes.NameIdentifier).Single().Value);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, "Token validation failed! Token is {} .", token);
                return null;
            }
        }
    }
}
