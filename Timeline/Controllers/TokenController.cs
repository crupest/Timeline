using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
            public const int Create_UserNotExist = 1001;
            public const int Create_BadPassword = 1002;

            public const int Verify_BadToken = 2001;
            public const int Verify_UserNotExist = 2002;
            public const int Verify_BadVersion = 2003;
        }

        private readonly IUserService _userService;
        private readonly ILogger<TokenController> _logger;

        public TokenController(IUserService userService, ILogger<TokenController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateTokenRequest request)
        {
            try
            {
                var result = await _userService.CreateToken(request.Username, request.Password);
                _logger.LogInformation(LoggingEventIds.LogInSucceeded, "Login succeeded. Username: {} .", request.Username);
                return Ok(new CreateTokenResponse
                {
                    Token = result.Token,
                    User = result.User
                });
            }
            catch(UserNotExistException e)
            {
                var code = ErrorCodes.Create_UserNotExist;
                _logger.LogInformation(LoggingEventIds.LogInFailed, e, "Attemp to login failed. Code: {} Username: {} Password: {} .", code, request.Username, request.Password);
                return BadRequest(new CommonErrorResponse(code, "Bad username or password."));
            }
            catch (BadPasswordException e)
            {
                var code = ErrorCodes.Create_BadPassword;
                _logger.LogInformation(LoggingEventIds.LogInFailed, e, "Attemp to login failed. Code: {} Username: {} Password: {} .", code, request.Username, request.Password);
                return BadRequest(new CommonErrorResponse(code, "Bad username or password."));
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
                var code = ErrorCodes.Verify_BadToken;
                _logger.LogInformation(LoggingEventIds.VerifyFailed, e, "Attemp to verify a bad token. Code: {} Token: {}.", code, request.Token);
                return BadRequest(new CommonErrorResponse(code, "A token of bad format."));
            }
            catch (UserNotExistException e)
            {
                var code = ErrorCodes.Verify_UserNotExist;
                _logger.LogInformation(LoggingEventIds.VerifyFailed, e, "Attemp to verify a bad token. Code: {} Token: {}.", code, request.Token);
                return BadRequest(new CommonErrorResponse(code, "The user does not exist. Administrator might have deleted this user."));
            }
            catch (BadTokenVersionException e)
            {
                var code = ErrorCodes.Verify_BadToken;
                _logger.LogInformation(LoggingEventIds.VerifyFailed, e, "Attemp to verify a bad token. Code: {} Token: {}.", code, request.Token);
                return BadRequest(new CommonErrorResponse(code, "The token is expired. Try recreate a token."));
            }
        }
    }
}
