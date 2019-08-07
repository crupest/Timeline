using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Entities.Http;
using Timeline.Services;
using static Timeline.Helpers.MyLogHelper;

namespace Timeline.Controllers
{
    [Route("token")]
    public class TokenController : Controller
    {
        private static class LoggingEventIds
        {
            public const int CreateSucceeded = 1000;
            public const int CreateFailed = 1001;

            public const int VerifySucceeded = 2000;
            public const int VerifyFailed = 2001;
        }

        public static class ErrorCodes
        {
            public const int Create_UserNotExist = -1001;
            public const int Create_BadPassword = -1002;
            public const int Create_BadExpireOffset = -1003;

            public const int Verify_BadToken = -2001;
            public const int Verify_UserNotExist = -2002;
            public const int Verify_BadVersion = -2003;
            public const int Verify_Expired = -2004;
        }

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
        public async Task<IActionResult> Create([FromBody] CreateTokenRequest request)
        {
            void LogFailure(string reason, int code, Exception e = null)
            {
                _logger.LogInformation(LoggingEventIds.CreateFailed, e, FormatLogMessage("Attemp to login failed.",
                    Pair("Reason", reason),
                    Pair("Code", code),
                    Pair("Username", request.Username),
                    Pair("Password", request.Password),
                    Pair("Expire Offset (in days)", request.ExpireOffset)));
            }

            TimeSpan? expireOffset = null;
            if (request.ExpireOffset != null)
            {
                if (request.ExpireOffset.Value <= 0.0)
                {
                    const string message = "Expire time is not bigger than 0.";
                    var code = ErrorCodes.Create_BadExpireOffset;
                    LogFailure(message, code);
                    return BadRequest(new CommonResponse(code, message));
                }
                expireOffset = TimeSpan.FromDays(request.ExpireOffset.Value);
            }

            try
            {
                var expiredTime = expireOffset == null ? null : (DateTime?)(_clock.GetCurrentTime() + expireOffset.Value);
                var result = await _userService.CreateToken(request.Username, request.Password, expiredTime);
                _logger.LogInformation(LoggingEventIds.CreateSucceeded, FormatLogMessage("Attemp to login succeeded.",
                    Pair("Username", request.Username),
                    Pair("Expire Time", expiredTime == null ? "default" : expiredTime.Value.ToString())));
                return Ok(new CreateTokenResponse
                {
                    Token = result.Token,
                    User = result.User
                });
            }
            catch (UserNotExistException e)
            {
                var code = ErrorCodes.Create_UserNotExist;
                LogFailure("User does not exist.", code, e);
                return BadRequest(new CommonResponse(code, "Bad username or password."));
            }
            catch (BadPasswordException e)
            {
                var code = ErrorCodes.Create_BadPassword;
                LogFailure("Password is wrong.", code, e);
                return BadRequest(new CommonResponse(code, "Bad username or password."));
            }
        }

        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<IActionResult> Verify([FromBody] VerifyTokenRequest request)
        {
            void LogFailure(string reason, int code, Exception e = null, params KeyValuePair<string, object>[] otherProperties)
            {
                var properties = new KeyValuePair<string, object>[3 + otherProperties.Length];
                properties[0] = Pair("Reason", reason);
                properties[1] = Pair("Code", code);
                properties[2] = Pair("Token", request.Token);
                otherProperties.CopyTo(properties, 3);
                _logger.LogInformation(LoggingEventIds.VerifyFailed, e, FormatLogMessage("Token verification failed.", properties));
            }

            try
            {
                var result = await _userService.VerifyToken(request.Token);
                _logger.LogInformation(LoggingEventIds.VerifySucceeded,
                    FormatLogMessage("Token verification succeeded.",
                    Pair("Username", result.Username), Pair("Token", request.Token)));
                return Ok(new VerifyTokenResponse
                {
                    User = result
                });
            }
            catch (JwtTokenVerifyException e)
            {
                if (e.ErrorCode == JwtTokenVerifyException.ErrorCodes.Expired)
                {
                    const string message = "Token is expired.";
                    var code = ErrorCodes.Verify_Expired;
                    var innerException = e.InnerException as SecurityTokenExpiredException;
                    LogFailure(message, code, e, Pair("Expires", innerException.Expires));
                    return BadRequest(new CommonResponse(code, message));
                }
                else
                {
                    const string message = "Token is of bad format.";
                    var code = ErrorCodes.Verify_BadToken;
                    LogFailure(message, code, e);
                    return BadRequest(new CommonResponse(code, message));
                }
            }
            catch (UserNotExistException e)
            {
                const string message = "User does not exist. Administrator might have deleted this user.";
                var code = ErrorCodes.Verify_UserNotExist;
                LogFailure(message, code, e);
                return BadRequest(new CommonResponse(code, message));
            }
            catch (BadTokenVersionException e)
            {
                const string message = "Token has a old version.";
                var code = ErrorCodes.Verify_BadVersion;
                LogFailure(message, code, e);
                _logger.LogInformation(LoggingEventIds.VerifyFailed, e, "Attemp to verify a bad token because version is old. Code: {} Token: {}.", code, request.Token);
                return BadRequest(new CommonResponse(code, message));
            }
        }
    }
}
