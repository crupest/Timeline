using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Models.Http;
using Timeline.Services;
using static Timeline.Resources.Authentication.AuthHandler;

namespace Timeline.Auth
{
    public static class AuthenticationConstants
    {
        public const string Scheme = "Bearer";
        public const string DisplayName = "My Jwt Auth Scheme";
        public const string PermissionClaimName = "Permission";
    }

    public class MyAuthenticationOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// The query param key to search for token. If null then query params are not searched for token. Default to <c>"token"</c>.
        /// </summary>
        public string TokenQueryParamKey { get; set; } = "token";
    }

    public class MyAuthenticationHandler : AuthenticationHandler<MyAuthenticationOptions>
    {
        private const string TokenErrorCodeKey = "TokenErrorCode";

        private static CommonResponse CreateChallengeResponseBody(int errorCode)
        {
            return new CommonResponse(errorCode, errorCode switch
            {
                ErrorCodes.Common.Token.TimeExpired => "The token is out of date and expired. Please create a new one.",
                ErrorCodes.Common.Token.VersionExpired => "The token is of old version and expired. Please create a new one.",
                ErrorCodes.Common.Token.BadFormat => "The token is of bad format. It might not be created by this server.",
                ErrorCodes.Common.Token.UserNotExist => "The owner of the token does not exist. It might have been deleted.",
                _ => "Unknown error."
            });
        }

        private readonly ILogger<MyAuthenticationHandler> _logger;
        private readonly IUserTokenManager _userTokenManager;
        private readonly IUserPermissionService _userPermissionService;

        private readonly IOptionsMonitor<JsonOptions> _jsonOptions;

        public MyAuthenticationHandler(IOptionsMonitor<MyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IUserTokenManager userTokenManager, IUserPermissionService userPermissionService, IOptionsMonitor<JsonOptions> jsonOptions)
            : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger<MyAuthenticationHandler>();
            _userTokenManager = userTokenManager;
            _userPermissionService = userPermissionService;
            _jsonOptions = jsonOptions;
        }

        // return null if no token is found
        private string? ExtractToken()
        {
            // check the authorization header
            string header = Request.Headers[HeaderNames.Authorization];
            if (!string.IsNullOrEmpty(header) && header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = header["Bearer ".Length..].Trim();
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
                var user = await _userTokenManager.VerifyToken(token);

                var identity = new ClaimsIdentity(AuthenticationConstants.Scheme);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64));
                identity.AddClaim(new Claim(identity.NameClaimType, user.Username, ClaimValueTypes.String));

                var permissions = await _userPermissionService.GetPermissionsOfUserAsync(user.Id);
                identity.AddClaims(permissions.Select(permission => new Claim(AuthenticationConstants.PermissionClaimName, permission.ToString(), ClaimValueTypes.String)));

                var principal = new ClaimsPrincipal();
                principal.AddIdentity(identity);

                return AuthenticateResult.Success(new AuthenticationTicket(principal, AuthenticationConstants.Scheme));
            }
            catch (Exception e) when (!(e is ArgumentException))
            {
                _logger.LogInformation(e, LogTokenValidationFail);
                return AuthenticateResult.Fail(e, new AuthenticationProperties(new Dictionary<string, string?>()
                {
                    [TokenErrorCodeKey] = (e switch
                    {
                        UserTokenTimeExpiredException => ErrorCodes.Common.Token.TimeExpired,
                        UserTokenVersionExpiredException => ErrorCodes.Common.Token.VersionExpired,
                        UserTokenBadFormatException => ErrorCodes.Common.Token.BadFormat,
                        UserTokenUserNotExistException => ErrorCodes.Common.Token.UserNotExist,
                        _ => ErrorCodes.Common.Token.Unknown
                    }).ToString(CultureInfo.InvariantCulture)
                }));
            }
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;

            CommonResponse body;

            if (properties.Items.TryGetValue(TokenErrorCodeKey, out var tokenErrorCode))
            {
                if (!int.TryParse(tokenErrorCode, out var errorCode))
                    errorCode = ErrorCodes.Common.Token.Unknown;
                body = CreateChallengeResponseBody(errorCode);
            }
            else
            {
                body = new CommonResponse(ErrorCodes.Common.Unauthorized, "You must use a token to authenticate.");
            }


            var bodyData = JsonSerializer.SerializeToUtf8Bytes(body, typeof(CommonResponse), _jsonOptions.CurrentValue.JsonSerializerOptions);

            Response.ContentType = MimeTypes.ApplicationJson;
            Response.ContentLength = bodyData.Length;
            await Response.Body.WriteAsync(bodyData);
        }
    }
}
