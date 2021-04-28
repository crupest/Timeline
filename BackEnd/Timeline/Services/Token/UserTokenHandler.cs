using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Timeline.Configs;
using Timeline.Entities;

namespace Timeline.Services.Token
{
    public class JwtUserTokenHandler : IUserTokenHandler
    {
        private const string VersionClaimType = "timeline_version";

        private readonly IOptionsMonitor<JwtOptions> _jwtConfig;
        private readonly IClock _clock;

        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
        private SymmetricSecurityKey _tokenSecurityKey;

        public JwtUserTokenHandler(IOptionsMonitor<JwtOptions> jwtConfig, IClock clock, DatabaseContext database)
        {
            _jwtConfig = jwtConfig;
            _clock = clock;

            var key = database.JwtToken.Select(t => t.Key).SingleOrDefault();

            if (key == null)
            {
                throw new InvalidOperationException(Resources.Services.UserTokenService.JwtKeyNotExist);
            }

            _tokenSecurityKey = new SymmetricSecurityKey(key);
        }

        public string GenerateToken(UserTokenInfo tokenInfo)
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
                SigningCredentials = new SigningCredentials(_tokenSecurityKey, SecurityAlgorithms.HmacSha384),
                IssuedAt = _clock.GetCurrentTime(),
                Expires = tokenInfo.ExpireAt,
                NotBefore = _clock.GetCurrentTime() // I must explicitly set this or it will use the current time by default and mock is not work in which case test will not pass.
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = _tokenHandler.WriteToken(token);

            return tokenString;
        }


        public UserTokenInfo VerifyToken(string token)
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
                    ValidateLifetime = false,
                    ValidIssuer = config.Issuer,
                    ValidAudience = config.Audience,
                    IssuerSigningKey = _tokenSecurityKey
                }, out var t);

                var idClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (idClaim == null)
                    throw new JwtUserTokenBadFormatException(token, JwtUserTokenBadFormatException.ErrorKind.NoIdClaim);
                if (!long.TryParse(idClaim, out var id))
                    throw new JwtUserTokenBadFormatException(token, JwtUserTokenBadFormatException.ErrorKind.IdClaimBadFormat);

                var versionClaim = principal.FindFirstValue(VersionClaimType);
                if (versionClaim == null)
                    throw new JwtUserTokenBadFormatException(token, JwtUserTokenBadFormatException.ErrorKind.NoVersionClaim);
                if (!long.TryParse(versionClaim, out var version))
                    throw new JwtUserTokenBadFormatException(token, JwtUserTokenBadFormatException.ErrorKind.VersionClaimBadFormat);

                var decodedToken = (JwtSecurityToken)t;
                var exp = decodedToken.Payload.Exp;
                if (exp is null)
                    throw new JwtUserTokenBadFormatException(token, JwtUserTokenBadFormatException.ErrorKind.NoExp);

                return new UserTokenInfo
                {
                    Id = id,
                    Version = version,
                    ExpireAt = EpochTime.DateTime(exp.Value)
                };
            }
            catch (Exception e) when (e is SecurityTokenException || e is ArgumentException)
            {
                throw new JwtUserTokenBadFormatException(token, JwtUserTokenBadFormatException.ErrorKind.Other, e);
            }
        }
    }
}
