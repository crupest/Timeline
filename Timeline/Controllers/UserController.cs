using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Timeline.Services;

namespace Timeline.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private static class LoggingEventIds
        {
            public const int LogInSucceeded = 4000;
            public const int LogInFailed = 4001;
        }

        public class UserCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class LoginInfo
        {
            public string Token { get; set; }
            public string[] Roles { get; set; }
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
        public ActionResult<LoginInfo> LogIn([FromBody] UserCredentials credentials)
        {
            var user = _userService.Authenticate(credentials.Username, credentials.Password);

            if (user == null) {
                _logger.LogInformation(LoggingEventIds.LogInFailed, "Attemp to login with username: {} and password: {} failed.", credentials.Username, credentials.Password);
                return BadRequest();
            }

            _logger.LogInformation(LoggingEventIds.LogInSucceeded, "Login with username: {} succeeded.", credentials.Username);

            var result = new LoginInfo
            {
                Token = _jwtService.GenerateJwtToken(user),
                Roles = user.Roles
            };

            return Ok(result);
        }
    }
}
