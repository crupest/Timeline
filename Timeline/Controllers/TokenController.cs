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
            public const int LogInSucceeded = 4000;
            public const int LogInFailed = 4001;
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
        public async Task<ActionResult<CreateTokenResponse>> Create([FromBody] CreateTokenRequest request)
        {
            var result = await _userService.CreateToken(request.Username, request.Password);

            if (result == null)
            {
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
                Token = result.Token,
                UserInfo = result.User
            });
        }

        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<ActionResult<VerifyTokenResponse>> Verify([FromBody] VerifyTokenRequest request)
        {
            var result = await _userService.VerifyToken(request.Token);

            if (result == null)
            {
                return Ok(new VerifyTokenResponse
                {
                    IsValid = false,
                });
            }

            return Ok(new VerifyTokenResponse
            {
                IsValid = true,
                UserInfo = result
            });
        }
    }
}
