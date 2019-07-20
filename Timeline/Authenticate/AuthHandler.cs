using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Timeline.Authenticate
{
    static class AuthConstants
    {
        public const string Scheme = "Bearer";
        public const string DisplayName = "My Jwt Auth Scheme";
    }

    class AuthOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// The query param key to search for token. If null then query params are not searched for token. Default to <c>"token"</c>.
        /// </summary>
        public string TokenQueryParamKey { get; set; } = "token";

        public TokenValidationParameters TokenValidationParameters { get;
            set; } 
            = new TokenValidationParameters();
    }

    class AuthHandler : AuthenticationHandler<AuthOptions>
    {
        private readonly ILogger<AuthHandler> _logger;

        public AuthHandler(IOptionsMonitor<AuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger<AuthHandler>();
        }

        // return null if no token is found
        private string ExtractToken()
        {
            // check the authorization header
            string header = Request.Headers[HeaderNames.Authorization];
            if (!string.IsNullOrEmpty(header) && header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = header.Substring("Bearer ".Length).Trim();
                _logger.LogInformation("Token is found in authorization header. Token is {} .", token);
                return token;
            }

            // check the query params
            var paramQueryKey = Options.TokenQueryParamKey;
            if (!string.IsNullOrEmpty(paramQueryKey))
            {
                string token = Request.Query[paramQueryKey];
                if (!string.IsNullOrEmpty(token))
                {
                    _logger.LogInformation("Token is found in query param with key \"{}\". Token is {} .", paramQueryKey, token);
                    return token;
                }
            }

            // not found anywhere then return null
            return null;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var token = ExtractToken();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogInformation("No jwt token is found.");
                return AuthenticateResult.NoResult();
            }

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var principal = handler.ValidateToken(token, Options.TokenValidationParameters, out var validatedToken);
                return AuthenticateResult.Success(new AuthenticationTicket(principal, AuthConstants.Scheme));
            }
            catch (SecurityTokenException e)
            {
                _logger.LogInformation(e, "A jwt token validation failed.");
                return AuthenticateResult.Fail(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Arguments passed to the JwtSecurityTokenHandler.ValidateToken are bad.");
                throw e;
            }
        }
    }
}
