using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Services;
using static Timeline.Resources.Authentication.AuthHandler;

namespace Timeline.Authentication
{
    static class AuthConstants
    {
        public const string Scheme = "Bearer";
        public const string DisplayName = "My Jwt Auth Scheme";
    }

    public class AuthOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// The query param key to search for token. If null then query params are not searched for token. Default to <c>"token"</c>.
        /// </summary>
        public string TokenQueryParamKey { get; set; } = "token";
    }

    public class AuthHandler : AuthenticationHandler<AuthOptions>
    {
        private readonly ILogger<AuthHandler> _logger;
        private readonly IUserService _userService;

        public AuthHandler(IOptionsMonitor<AuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IUserService userService)
            : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger<AuthHandler>();
            _userService = userService;
        }

        // return null if no token is found
        private string? ExtractToken()
        {
            // check the authorization header
            string header = Request.Headers[HeaderNames.Authorization];
            if (!string.IsNullOrEmpty(header) && header.StartsWith("Bearer ", StringComparison.InvariantCultureIgnoreCase))
            {
                var token = header.Substring("Bearer ".Length).Trim();
                _logger.LogInformation(LogTokenFoundInHeader, token);
                return token;
            }

            // check the query params
            var paramQueryKey = Options.TokenQueryParamKey;
            if (!string.IsNullOrEmpty(paramQueryKey))
            {
                string token = Request.Query[paramQueryKey];
                if (!string.IsNullOrEmpty(token))
                {
                    _logger.LogInformation(LogTokenFoundInQuery, paramQueryKey, token);
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
                _logger.LogInformation(LogTokenNotFound);
                return AuthenticateResult.NoResult();
            }

            try
            {
                var userInfo = await _userService.VerifyToken(token);

                var identity = new ClaimsIdentity(AuthConstants.Scheme);
                identity.AddClaim(new Claim(identity.NameClaimType, userInfo.Username, ClaimValueTypes.String));
                identity.AddClaims(UserRoleConvert.ToArray(userInfo.Administrator).Select(role => new Claim(identity.RoleClaimType, role, ClaimValueTypes.String)));

                var principal = new ClaimsPrincipal();
                principal.AddIdentity(identity);

                return AuthenticateResult.Success(new AuthenticationTicket(principal, AuthConstants.Scheme));
            }
            catch (Exception e) when (!(e is ArgumentException))
            {
                _logger.LogInformation(e, LogTokenValidationFail);
                return AuthenticateResult.Fail(e);
            }
        }
    }
}
