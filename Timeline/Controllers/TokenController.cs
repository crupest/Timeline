using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Services;
using Timeline.Helpers;

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
        public async Task<IActionResult> Create([FromBody] CreateTokenRequest request)
        {
            void LogFailure(string reason, Exception? e = null)
            {
                _logger.LogInformation(e, Log.Format("Attemp to login failed.",
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

                _logger.LogInformation(Log.Format("Attemp to login succeeded.",
                    ("Username", request.Username),
                    ("Expire At", expireTime?.ToString() ?? "default")
                ));
                return Ok(new CreateTokenResponse
                {
                    Token = result.Token,
                    User = result.User
                });
            }
            catch (UserNotExistException e)
            {
                LogFailure("User does not exist.", e);
                return BadRequest(new CommonResponse(ErrorCodes.Http.Token.Create.BadCredential,
                    "Bad username or password."));
            }
            catch (BadPasswordException e)
            {
                LogFailure("Password is wrong.", e);
                return BadRequest(new CommonResponse(ErrorCodes.Http.Token.Create.BadCredential,
                    "Bad username or password."));
            }
        }

        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<IActionResult> Verify([FromBody] VerifyTokenRequest request)
        {
            void LogFailure(string reason, Exception? e = null, params (string, object?)[] otherProperties)
            {
                var properties = new (string, object)[2 + otherProperties.Length];
                properties[0] = ("Reason", reason);
                properties[1] = ("Token", request.Token);
                otherProperties.CopyTo(properties, 2);
                _logger.LogInformation(e, Log.Format("Token verification failed.", properties));
            }

            try
            {
                var result = await _userService.VerifyToken(request.Token);
                _logger.LogInformation(Log.Format("Token verification succeeded.",
                    ("Username", result.Username), ("Token", request.Token)));
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
                    var innerException = e.InnerException as SecurityTokenExpiredException;
                    LogFailure(message, e, ("Expires", innerException?.Expires), ("Current Time", _clock.GetCurrentTime()));
                    return BadRequest(new CommonResponse(ErrorCodes.Http.Token.Verify.Expired, message));
                }
                else
                {
                    const string message = "Token is of bad format.";
                    LogFailure(message, e);
                    return BadRequest(new CommonResponse(ErrorCodes.Http.Token.Verify.BadFormat, message));
                }
            }
            catch (UserNotExistException e)
            {
                const string message = "User does not exist. Administrator might have deleted this user.";
                LogFailure(message, e);
                return BadRequest(new CommonResponse(ErrorCodes.Http.Token.Verify.UserNotExist, message));
            }
            catch (BadTokenVersionException e)
            {
                const string message = "Token has an old version.";
                LogFailure(message, e, ("Token Version", e.TokenVersion), ("Required Version", e.RequiredVersion));
                return BadRequest(new CommonResponse(ErrorCodes.Http.Token.Verify.OldVersion, message));
            }
        }
    }
}
