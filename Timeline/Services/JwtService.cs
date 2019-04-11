using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Timeline.Configs;
using Timeline.Entities;

namespace Timeline.Services
{
    public interface IJwtService
    {
        /// <summary>
        /// Create a JWT token for a given user.
        /// Return null if <paramref name="user"/> is null.
        /// </summary>
        /// <param name="user">The user to generate token.</param>
        /// <returns>The generated token or null if <paramref name="user"/> is null.</returns>
        string GenerateJwtToken(User user);

        /// <summary>
        /// Validate a JWT token.
        /// Return null is <paramref name="token"/> is null.
        /// If token is invalid, return a <see cref="TokenValidationResponse"/> with
        /// <see cref="TokenValidationResponse.IsValid"/> set to false and
        /// <see cref="TokenValidationResponse.UserInfo"/> set to null.
        /// If token is valid, return a <see cref="TokenValidationResponse"/> with
        /// <see cref="TokenValidationResponse.IsValid"/> set to true and
        /// <see cref="TokenValidationResponse.UserInfo"/> filled with the user info
        /// in the token.
        /// </summary>
        /// <param name="token">The token string to validate.</param>
        /// <returns>Null if <paramref name="token"/> is null. Or the result.</returns>
        TokenValidationResponse ValidateJwtToken(string token);

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

        public string GenerateJwtToken(User user)
        {
            if (user == null)
                return null;

            var jwtConfig = _jwtConfig.CurrentValue;

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            identity.AddClaim(new Claim(identity.NameClaimType, user.Username));
            identity.AddClaims(user.Roles.Select(role => new Claim(identity.RoleClaimType, role)));

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


        public TokenValidationResponse ValidateJwtToken(string token)
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

                var identity = principal.Identity as ClaimsIdentity;

                var userInfo = new UserInfo
                {
                    Username = identity.FindAll(identity.NameClaimType).Select(claim => claim.Value).Single(),
                    Roles = identity.FindAll(identity.RoleClaimType).Select(claim => claim.Value).ToArray()
                };

                return new TokenValidationResponse
                {
                    IsValid = true,
                    UserInfo = userInfo
                };
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, "Token validation failed! Token is {} .", token);
                return new TokenValidationResponse { IsValid = false };
            }
        }
    }
}
