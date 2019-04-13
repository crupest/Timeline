using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Timeline.Entities;
using Timeline.Services;

namespace Timeline.Controllers
{
    [Route("[controller]")]
    public class UserController : Controller
    {
        private static class LoggingEventIds
        {
            public const int LogInSucceeded = 4000;
            public const int LogInFailed = 4001;
        }

        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, IJwtService jwtService, ILogger<UserController> logger)
        {
            _userService = userService;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public ActionResult<CreateTokenResponse> CreateToken([FromBody] CreateTokenRequest request)
        {
            var user = _userService.Authenticate(request.Username, request.Password);

            if (user == null) {
                _logger.LogInformation(LoggingEventIds.LogInFailed, "Attemp to login with username: {} and password: {} failed.", request.Username, request.Password);
                return Ok(new CreateTokenResponse
                {
                    Success = false
                });
            }

            _logger.LogInformation(LoggingEventIds.LogInSucceeded, "Login with username: {} succeeded.", request.Username);

            return Ok(new CreateTokenResponse
            {
                Success = true,
                Token = _jwtService.GenerateJwtToken(user),
                UserInfo = user.GetUserInfo()
            });
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public ActionResult<TokenValidationResponse> ValidateToken([FromBody] TokenValidationRequest request)
        {
            var result = _jwtService.ValidateJwtToken(request.Token);
            return Ok(result);
        }
    }
}
