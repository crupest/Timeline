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
using Timeline.Services.Token;
using Timeline.Services.User;

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

        private static int GetErrorCodeForUserTokenException(UserTokenException e)
        {
            return e switch
            {
                UserTokenExpiredException => ErrorCodes.Common.Token.TimeExpired,
                _ => ErrorCodes.Common.Token.Invalid
            };
        }

        private static string GetTokenErrorMessageFromErrorCode(int errorCode)
        {
            return errorCode switch
            {
                ErrorCodes.Common.Token.TimeExpired => Resource.MessageTokenExpired,
                _ => Resource.MessageTokenInvalid
            };
        }

        private readonly ILogger<MyAuthenticationHandler> _logger;
        private readonly IUserTokenService _userTokenService;
        private readonly IUserPermissionService _userPermissionService;

        private readonly IOptionsMonitor<JsonOptions> _jsonOptions;

        public MyAuthenticationHandler(IOptionsMonitor<MyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IUserTokenService userTokenService, IUserPermissionService userPermissionService, IOptionsMonitor<JsonOptions> jsonOptions)
            : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger<MyAuthenticationHandler>();
            _userTokenService = userTokenService;
            _userPermissionService = userPermissionService;
            _jsonOptions = jsonOptions;
        }

        // return null if no token is found
        private string? ExtractToken()
        {
            // check the authorization header
            string? header = Request.Headers[HeaderNames.Authorization];
            if (!string.IsNullOrEmpty(header) && header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = header["Bearer ".Length..].Trim();
                _logger.LogInformation(Resource.LogTokenFoundInHeader, token);
                return token;
            }

            // check the query params
            var paramQueryKey = Options.TokenQueryParamKey;
            if (!string.IsNullOrEmpty(paramQueryKey))
            {
                string? token = Request.Query[paramQueryKey];
                if (!string.IsNullOrEmpty(token))
                {
                    _logger.LogInformation(Resource.LogTokenFoundInQuery, paramQueryKey, token);
                    return token;
                }
            }

            {
                var token = Request.Query["access_token"];
                var path = Context.Request.Path;

                if (!string.IsNullOrEmpty(token) && path.StartsWithSegments("/api/hub"))
                {
                    _logger.LogInformation(Resource.LogTokenFoundInQuery, "access_token", token);
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
                _logger.LogInformation(Resource.LogTokenNotFound);
                return AuthenticateResult.NoResult();
            }

            try
            {
                var userTokenInfo = await _userTokenService.ValidateTokenAsync(token);

                var identity = new ClaimsIdentity(AuthenticationConstants.Scheme);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userTokenInfo.UserId.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64));

                var permissions = await _userPermissionService.GetPermissionsOfUserAsync(userTokenInfo.UserId);
                identity.AddClaims(permissions.Select(permission => new Claim(AuthenticationConstants.PermissionClaimName, permission.ToString(), ClaimValueTypes.String)));

                var principal = new ClaimsPrincipal();
                principal.AddIdentity(identity);

                return AuthenticateResult.Success(new AuthenticationTicket(principal, AuthenticationConstants.Scheme));
            }
            catch (UserTokenException e)
            {
                var errorCode = GetErrorCodeForUserTokenException(e);

                _logger.LogInformation(e, Resource.LogTokenValidationFail, GetTokenErrorMessageFromErrorCode(errorCode));
                return AuthenticateResult.Fail(e, new AuthenticationProperties(new Dictionary<string, string?>()
                {
                    [TokenErrorCodeKey] = errorCode.ToString(CultureInfo.InvariantCulture)
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
                    throw new Exception("A logic error: failed to parse token error code.");
                body = new CommonResponse(errorCode, GetTokenErrorMessageFromErrorCode(errorCode));
            }
            else
            {
                body = new CommonResponse(ErrorCodes.Common.Unauthorized, Resource.MessageNoToken);
            }

            var bodyData = JsonSerializer.SerializeToUtf8Bytes(body, typeof(CommonResponse), _jsonOptions.CurrentValue.JsonSerializerOptions);

            Response.ContentType = MimeTypes.ApplicationJson;
            Response.ContentLength = bodyData.Length;
            await Response.Body.WriteAsync(bodyData);
        }
    }
}
