using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Timeline.Helpers;
using Timeline.Models.Http;
using Timeline.Services;
using static Timeline.Resources.Controllers.TokenController;

namespace Timeline
{
    public static partial class ErrorCodes
    {
        public static partial class Http
        {
            public static class Token // bbb = 001
            {
                public static class Create // cc = 01
                {
                    public const int BadCredential = 10010101;
                }

                public static class Verify // cc = 02
                {
                    public const int BadFormat = 10010201;
                    public const int UserNotExist = 10010202;
                    public const int OldVersion = 10010203;
                    public const int Expired = 10010204;
                }
            }
        }
    }
}

namespace Timeline.Controllers
{
    [Route("token")]
    [ApiController]
    public class TokenController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<TokenController> _logger;
        private readonly IClock _clock;

        public TokenController(IUserService userService, ILogger<TokenController> logger, IClock clock)
        {
            _userService = userService;
            _logger = logger;
            _clock = clock;
        }

        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<ActionResult<CreateTokenResponse>> Create([FromBody] CreateTokenRequest request)
        {
            void LogFailure(string reason, Exception? e = null)
            {
                _logger.LogInformation(e, Log.Format(LogCreateFailure,
                    ("Reason", reason),
                    ("Username", request.Username),
                    ("Password", request.Password),
                    ("Expire (in days)", request.Expire)
                ));
            }

            try
            {
                DateTime? expireTime = null;
                if (request.Expire != null)
                    expireTime = _clock.GetCurrentTime().AddDays(request.Expire.Value);

                var result = await _userService.CreateToken(request.Username, request.Password, expireTime);

                _logger.LogInformation(Log.Format(LogCreateSuccess,
                    ("Username", request.Username),
                    ("Expire At", expireTime?.ToString(CultureInfo.CurrentCulture.DateTimeFormat) ?? "default")
                ));
                return Ok(new CreateTokenResponse
                {
                    Token = result.Token,
                    User = result.User
                });
            }
            catch (UserNotExistException e)
            {
                LogFailure(LogUserNotExist, e);
                return BadRequest(new CommonResponse(
                    ErrorCodes.Http.Token.Create.BadCredential,
                    ErrorBadCredential));
            }
            catch (BadPasswordException e)
            {
                LogFailure(LogBadPassword, e);
                return BadRequest(new CommonResponse(
                    ErrorCodes.Http.Token.Create.BadCredential,
                     ErrorBadCredential));
            }
        }

        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<ActionResult<VerifyTokenResponse>> Verify([FromBody] VerifyTokenRequest request)
        {
            void LogFailure(string reason, Exception? e = null, params (string, object?)[] otherProperties)
            {
                var properties = new (string, object?)[2 + otherProperties.Length];
                properties[0] = ("Reason", reason);
                properties[1] = ("Token", request.Token);
                otherProperties.CopyTo(properties, 2);
                _logger.LogInformation(e, Log.Format(LogVerifyFailure, properties));
            }

            try
            {
                var result = await _userService.VerifyToken(request.Token);
                _logger.LogInformation(Log.Format(LogVerifySuccess,
                    ("Username", result.Username), ("Token", request.Token)));
                return Ok(new VerifyTokenResponse
                {
                    User = result
                });
            }
            catch (JwtVerifyException e)
            {
                if (e.ErrorCode == JwtVerifyException.ErrorCodes.Expired)
                {
                    var innerException = e.InnerException as SecurityTokenExpiredException;
                    LogFailure(LogVerifyExpire, e, ("Expires", innerException?.Expires),
                        ("Current Time", _clock.GetCurrentTime()));
                    return BadRequest(new CommonResponse(
                        ErrorCodes.Http.Token.Verify.Expired, ErrorVerifyExpire));
                }
                else if (e.ErrorCode == JwtVerifyException.ErrorCodes.OldVersion)
                {
                    var innerException = e.InnerException as JwtBadVersionException;
                    LogFailure(LogVerifyOldVersion, e,
                        ("Token Version", innerException?.TokenVersion), ("Required Version", innerException?.RequiredVersion));
                    return BadRequest(new CommonResponse(
                        ErrorCodes.Http.Token.Verify.OldVersion, ErrorVerifyOldVersion));
                }
                else
                {
                    LogFailure(LogVerifyBadFormat, e);
                    return BadRequest(new CommonResponse(
                        ErrorCodes.Http.Token.Verify.BadFormat, ErrorVerifyBadFormat));
                }
            }
            catch (UserNotExistException e)
            {
                LogFailure(LogVerifyUserNotExist, e);
                return BadRequest(new CommonResponse(
                    ErrorCodes.Http.Token.Verify.UserNotExist, ErrorVerifyUserNotExist));
            }
        }
    }
}
