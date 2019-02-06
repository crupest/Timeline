using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
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
    }

    public class JwtService : IJwtService
    {
        private readonly IOptionsMonitor<JwtConfig> _jwtConfig;
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();

        public JwtService(IOptionsMonitor<JwtConfig> jwtConfig)
        {
            _jwtConfig = jwtConfig;
        }

        public string GenerateJwtToken(User user)
        {
            if (user == null)
                return null;

            var jwtConfig = _jwtConfig.CurrentValue;

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
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
    }
}
