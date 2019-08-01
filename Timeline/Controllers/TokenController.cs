using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Timeline.Entities.Http;
using Timeline.Services;

namespace Timeline.Controllers
{
    [Route("token")]
    public class TokenController : Controller
    {
        private static class LoggingEventIds
        {
            public const int LogInSucceeded = 1000;
            public const int LogInFailed = 1001;

            public const int VerifySucceeded = 2000;
            public const int VerifyFailed = 2001;
        }

        private static class ErrorCodes
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
            TimeSpan? expireOffset = null;
            if (request.ExpireOffset != null)
            {
                if (request.ExpireOffset.Value <= 0.0)
                {
                    var code = ErrorCodes.Create_BadExpireOffset;
                    _logger.LogInformation(LoggingEventIds.LogInFailed, "Attemp to login failed because expire time offset is bad. Code: {} Username: {} Password: {} Bad Expire Offset: {}.", code, request.Username, request.Password, request.ExpireOffset);
                    return BadRequest(new CommonResponse(code, "Expire time is not bigger than 0."));
                }
                expireOffset = TimeSpan.FromDays(request.ExpireOffset.Value);
            }

            try
            {
                var result = await _userService.CreateToken(request.Username, request.Password, expireOffset == null ? null : (DateTime?)(_clock.GetCurrentTime() + expireOffset.Value));
                _logger.LogInformation(LoggingEventIds.LogInSucceeded, "Login succeeded. Username: {} Expire Time Offset: {} days.", request.Username, request.ExpireOffset);
                return Ok(new CreateTokenResponse
                {
                    Token = result.Token,
                    User = result.User
                });
            }
            catch (UserNotExistException e)
            {
                var code = ErrorCodes.Create_UserNotExist;
                _logger.LogInformation(LoggingEventIds.LogInFailed, e, "Attemp to login failed because user does not exist. Code: {} Username: {} Password: {} .", code, request.Username, request.Password);
                return BadRequest(new CommonResponse(code, "Bad username or password."));
            }
            catch (BadPasswordException e)
            {
                var code = ErrorCodes.Create_BadPassword;
                _logger.LogInformation(LoggingEventIds.LogInFailed, e, "Attemp to login failed because password is wrong. Code: {} Username: {} Password: {} .", code, request.Username, request.Password);
                return BadRequest(new CommonResponse(code, "Bad username or password."));
            }
        }

        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<IActionResult> Verify([FromBody] VerifyTokenRequest request)
        {
            try
            {
                var result = await _userService.VerifyToken(request.Token);
                _logger.LogInformation(LoggingEventIds.VerifySucceeded, "Verify token succeeded. Username: {} Token: {} .", result.Username, request.Token);
                return Ok(new VerifyTokenResponse
                {
                    User = result
                });
            }
            catch (JwtTokenVerifyException e)
            {
                if (e.ErrorCode == JwtTokenVerifyException.ErrorCodes.Expired)
                {
                    var code = ErrorCodes.Verify_Expired;
                    _logger.LogInformation(LoggingEventIds.VerifyFailed, e, "Attemp to verify a expired token. Code: {} Token: {}.", code, request.Token);
                    return BadRequest(new CommonResponse(code, "A expired token."));
                }
                else
                {
                    var code = ErrorCodes.Verify_BadToken;
                    _logger.LogInformation(LoggingEventIds.VerifyFailed, e, "Attemp to verify a bad token because of bad format. Code: {} Token: {}.", code, request.Token);
                    return BadRequest(new CommonResponse(code, "A token of bad format."));
                }
            }
            catch (UserNotExistException e)
            {
                var code = ErrorCodes.Verify_UserNotExist;
                _logger.LogInformation(LoggingEventIds.VerifyFailed, e, "Attemp to verify a bad token because user does not exist. Code: {} Token: {}.", code, request.Token);
                return BadRequest(new CommonResponse(code, "The user does not exist. Administrator might have deleted this user."));
            }
            catch (BadTokenVersionException e)
            {
                var code = ErrorCodes.Verify_BadToken;
                _logger.LogInformation(LoggingEventIds.VerifyFailed, e, "Attemp to verify a bad token because version is old. Code: {} Token: {}.", code, request.Token);
                return BadRequest(new CommonResponse(code, "The token is expired. Try recreate a token."));
            }
        }
    }
}
